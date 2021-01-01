using Api.Hosting.AdminAPI;
using Api.Hosting.Dto;
using Api.Hosting.Helpers;
using Api.Hosting.Settings;
using Api.Service.Mongo;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : Controller
    {
        private ILogger _logger;
        private TokensFactory _tokensFactory;
        private SsoSettings _ssoSettings;
        private UsersService _usersService;
        private UserInvitationsService _invitatonsService;
        private const string realm = "cookwi2";

        public UsersController(ILogger<UsersController> logger, TokensFactory factory, IOptions<SsoSettings> ssoSettings, 
                               UsersService usersService, UserInvitationsService userInvitationsService)
        {
            _logger = logger;
            _tokensFactory = factory;
            _ssoSettings = ssoSettings?.Value;
            _usersService = usersService;
            _invitatonsService = userInvitationsService;
        }

        [HttpGet("me")]
        [Authorize]
        [SwaggerOperation(Summary = "Get profile")]
        [SwaggerResponse(200, "User", typeof(UserDto))]
        [SwaggerResponse(500, "A server error occured")]
        public async Task<IActionResult> Me()
        {
            try
            {
                var currentUser = HttpContext.User;
                var client = new KeycloakClient(_ssoSettings.Api.BaseUrl, () => _tokensFactory.AccessToken);
                var userId = UserHelper.GetId(currentUser);
                if (userId == null || userId == "")
                {
                    _logger.LogError($"Impossible to retrieve user id for an authenticated user (has at least a token), this is weird !");
                    return StatusCode(500, "An error occured getting the user informations");
                }
                var user = await client.GetUserAsync(realm, userId);

                return Ok(user.ToDto());
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error has occured getting user infos - {e}");
                return StatusCode(500, "An error occured getting the user informations");
            }
        }

        [HttpPost("register/{invitationId}")]
        [SwaggerOperation(Summary = "Register a new user")]
        [SwaggerResponse(201, "User created", typeof(UserDto))]
        [SwaggerResponse(400, "User is not well formatted")]
        [SwaggerResponse(401, "Not authorized")]
        [SwaggerResponse(403, "Invitation is no longer valid")]
        [SwaggerResponse(404, "Invitation not found")]
        [SwaggerResponse(409, "A user with this email already exists")]
        [SwaggerResponse(500, "Server error")]
        public async Task<IActionResult> Register(string invitationId, [FromBody] UserDto user)
        {
            // we first check the invitation Guid
            var inv = _invitatonsService.GetOne(invitationId);
            if (inv == null)
            {
                return NotFound();
            }

            if (inv.Expiration < DateTime.UtcNow)
            {
                return Forbid("Invitation has expired");
            }
            if (inv.Used)
            {
                return Forbid("Invitation has been used and is no longer valid");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new KeycloakClient(_ssoSettings.Api.BaseUrl, () => _tokensFactory.AccessToken);
            bool res;

            try
            {
                res = await client.CreateUserAsync(realm, new User
                {
                    Email = user.Email,
                    EmailVerified = false,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Enabled = true,
                    UserName = user.Email
                });
            }
            catch (Flurl.Http.FlurlHttpException e)
            {
                if (e.Call.HttpStatus == System.Net.HttpStatusCode.Conflict)
                {
                    return StatusCode(409, "An account with this email already exists");
                }
                else
                {
                    return StatusCode((int)e.Call.Response.StatusCode, e.Call.Response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error creating the user in SSO - {e}");
                return StatusCode(500, "An unexpected error has occured creating the user into our SSO, sorry.");
            }

            // get data from SSO (user id and group id)
            var ssoUser = (await client.GetUsersAsync(realm, email: user.Email, firstName: user.FirstName, lastName: user.LastName)).FirstOrDefault();
            var userGroup = (await client.GetGroupHierarchyAsync(realm, search: "users")).FirstOrDefault();
            if (ssoUser == null || userGroup == null)
            {
                return await ReturnErrorAndCleanUser(500, "An error occured getting back user or user group from SSO ... Sorry.", ssoUser?.Id);
            }

            // set user PASSWORD
            res = await client.ResetUserPasswordAsync(realm, ssoUser.Id, user.Password, false);
            if (!res)
            {
                return await ReturnErrorAndCleanUser(500, $"Cannot set password into SSO for user {user.Email}", ssoUser.Id);
            }

            // set user GROUP
            res = await client.UpdateUserGroupAsync(realm, ssoUser.Id, userGroup.Id, userGroup);
            if (!res)
            {
                return await ReturnErrorAndCleanUser(500, $"Cannot assign group Users to created user {user.Email}", ssoUser.Id);
            }

            // finally send the email VALIDATION
            using (var httpclient = new HttpClient())
            {
                httpclient.BaseAddress = new Uri(_ssoSettings.Api.BaseUrl);
                httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_tokensFactory.AccessToken}");
                httpclient.DefaultRequestHeaders.Add("Accept", "application/json");
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var resp = await httpclient.PutAsync($"/auth/admin/realms/cookwi2/users/{ssoUser.Id}/send-verify-email?client_id=cookwi-webclient&redirect_uri={HttpUtility.UrlEncode(_ssoSettings.Api.RedirectUrl)}", content);
                if (!resp.IsSuccessStatusCode)
                {
                    return await ReturnErrorAndCleanUser(500, $"Unable to send verification mail for address {user.Email} ... Sorry.", ssoUser.Id);
                }
            }
            // NOT WORKING ...
            //res = await client.VerifyUserEmailAddressAsync(realm, ssoUser.Id);
            //if (!res)
            //{
            //    return StatusCode(500, $"Unable to send verification mail for address {user.Email} ... Sorry.");
            //}

            inv.Used = true;
            _invitatonsService.Update(inv);

            // we create the user in our systems
            _usersService.Create(new Service.Models.User
            {
                SsoId = Guid.Parse(ssoUser.Id)
            });

            return Created(HttpContext.Request.Path, ssoUser.ToDto());
        }

        private async Task<IActionResult> ReturnErrorAndCleanUser(int statusCode, string message = null, string userId = null)
        {
            // let's try to clean the user
            if (userId != null)
            {
                var client = new KeycloakClient(_ssoSettings.Api.BaseUrl, () => _tokensFactory.AccessToken);
                var res = await client.DeleteUserAsync(realm, userId);
                if (!res)
                {
                    // here, we need to log something
                }
            }

            return StatusCode(statusCode, message);
        }
    }
}