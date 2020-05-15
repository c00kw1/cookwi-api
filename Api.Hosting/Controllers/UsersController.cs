using Api.Hosting.Dto;
using Api.Hosting.Helpers;
using Api.Hosting.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class UsersController : Controller
    {
        private AuthenticationSettings _authSettings;

        public UsersController(IOptions<AuthenticationSettings> authSettings)
        {
            _authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
        }

        /// <summary>
        /// Return current user's informations from SSO
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [SwaggerOperation(Summary = "[NOT WORKING] Retrieve current user informations from sso")]
        [SwaggerResponse(200, "User informations", typeof(UserDto))]
        [SwaggerResponse(400, "Cannot retrieve user informations")]
        public ActionResult<UserDto> GetMyUserInfo()
        {
            // to do that we need the user current Bearer token
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var token = HttpContext.GetTokenAsync("access_token").Result;
            var client = new RestClient(_authSettings.Domain);
            var req = new RestRequest(_authSettings.Routes["UserInfo"], Method.GET);
            req.AddHeader("Authorization", $"Bearer {token}");
            // request crafted, BOOM
            var res = client.Execute(req);

            if (!res.CheckResponse())
            {
                return BadRequest("Something went wrong gathering the user information. Sorry.");
            }
            try
            {
                // we check we can deserialize it, meaning the response format and content are pretty OK
                var infos = JsonSerializer.Deserialize<UserDto>(res.Content);
                return Ok(infos);
            }
            catch
            {
                return BadRequest("Something went wrong deserializing the user information. There may be a data corruption.");
            }
        }
    }
}