using Api.Hosting.AdminAPI;
using Api.Hosting.Dto;
using Api.Hosting.Settings;
using Api.Service;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.AspNetCore.Mvc;
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
        private TokensFactory _tokensFactory;
        private SsoSettings _ssoSettings;
        private CookwiDbContext _dbContext;

        public UsersController(TokensFactory factory, IOptions<SsoSettings> ssoSettings, CookwiDbContext dbContext)
        {
            _tokensFactory = factory;
            _ssoSettings = ssoSettings?.Value;
            _dbContext = dbContext;
        }

        [HttpPost("register/{invitationId}")]
        [SwaggerOperation(Summary = "Register a new user")]
        [SwaggerResponse(201, "User created", typeof(UserDto))]
        [SwaggerResponse(401, "Not authorized")]
        [SwaggerResponse(500, "Server error")]
        public async Task<IActionResult> Register(Guid invitationId, [FromBody] UserDto user)
        {
            // we first check the invitation Guid
            var invitation = _dbContext.UserInvitations.FirstOrDefault(i => i.Id == invitationId && !i.Used && i.DateExpiration > DateTime.Now);
            if (invitation == null)
            {
                return Unauthorized($"Invitation code {invitationId} is not valid (can be wrong, already used or just expired). Ask a new one.");
            }

            var client = new KeycloakClient(_ssoSettings.Api.BaseUrl, () => _tokensFactory.AccessToken);
            bool res;

            try
            {
                res = await client.CreateUserAsync("cookwi", new User
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

            // get data from SSO (user id and group id)
            var ssoUser = (await client.GetUsersAsync("cookwi", email: user.Email, firstName: user.FirstName, lastName: user.LastName)).FirstOrDefault();
            var userGroup = (await client.GetGroupHierarchyAsync("cookwi", search: "users")).FirstOrDefault();
            if (ssoUser == null || userGroup == null)
            {
                return await ReturnErrorAndCleanUser(500, "An error occured getting back user or user group from SSO ... Sorry.", ssoUser?.Id);
            }

            // set user PASSWORD
            res = await client.ResetUserPasswordAsync("cookwi", ssoUser.Id, user.Password, false);
            if (!res)
            {
                return await ReturnErrorAndCleanUser(500, $"Cannot set password into SSO for user {user.Email}", ssoUser.Id);
            }

            // set user GROUP
            res = await client.UpdateUserGroupAsync("cookwi", ssoUser.Id, userGroup.Id, userGroup);
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
                var resp = await httpclient.PutAsync($"/auth/admin/realms/cookwi/users/{ssoUser.Id}/send-verify-email?client_id=cookwi-webclient&redirect_uri={HttpUtility.UrlEncode(_ssoSettings.Api.RedirectUrl)}", content);
                if (!resp.IsSuccessStatusCode)
                {
                    return await ReturnErrorAndCleanUser(500, $"Unable to send verification mail for address {user.Email} ... Sorry.", ssoUser.Id);
                }
            }
            // NOT WORKING ...
            //res = await client.VerifyUserEmailAddressAsync("cookwi", ssoUser.Id);
            //if (!res)
            //{
            //    return StatusCode(500, $"Unable to send verification mail for address {user.Email} ... Sorry.");
            //}

            invitation.Used = true;
            // we create the user in our tables
            _dbContext.Users.Add(new Service.Models.User {
                Id = Guid.Parse(ssoUser.Id)
            });
            await _dbContext.SaveChangesAsync();

            return Created(HttpContext.Request.Path, ssoUser.ToDto());
        }

        private async Task<IActionResult> ReturnErrorAndCleanUser(int statusCode, string message = null, string userId = null)
        {
            // let's try to clean the user
            if (userId != null)
            {
                var client = new KeycloakClient(_ssoSettings.Api.BaseUrl, () => _tokensFactory.AccessToken);
                var res = await client.DeleteUserAsync("cookwi", userId);
                if (!res)
                {
                    // here, we need to log something
                }
            }

            return StatusCode(statusCode, message);
        }
    }
}