using Api.Hosting.AdminAPI;
using Api.Hosting.Dto.Admin;
using Api.Service;
using Api.Service.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace Api.Hosting.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Produces("application/json")]
    [Authorize("admin")]
    public class AdminUsersController : Controller
    {
        private TokensFactory _tokensFactory;
        private CookwiDbContext _dbContext;

        public AdminUsersController(TokensFactory factory, CookwiDbContext context)
        {
            _tokensFactory = factory;
            _dbContext = context;
        }

        [HttpGet("invitations/create")]
        [SwaggerOperation("Get an invitation code (the ID) to register")]
        [SwaggerResponse(201, "The invitation asked", typeof(UserInvitationDto))]
        [SwaggerResponse(500, "An error occured")]
        public async Task<IActionResult> GetInvitation()
        {
            var entity = await _dbContext.UserInvitations.AddAsync(UserInvitation.GenerateNew());
            await _dbContext.SaveChangesAsync();
            return Ok(entity.Entity.Dto());
        }
    }
}