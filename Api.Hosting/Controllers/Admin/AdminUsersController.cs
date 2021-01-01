using Api.Hosting.Dto.Admin;
using Api.Service.Models.Admin;
using Api.Service.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Api.Hosting.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Produces("application/json")]
    [Authorize(Constants.Policies.Admin)]
    public class AdminUsersController : Controller
    {
        private ILogger _logger;
        private UserInvitationsService _service;

        public AdminUsersController(ILogger<AdminUsersController> logger, UserInvitationsService _invitationsService)
        {
            _logger = logger;
            _service = _invitationsService;
        }

        #region Invitations

        [HttpGet("invitations/create")]
        [SwaggerOperation("Get an invitation code (the ID) to register")]
        [SwaggerResponse(201, "The invitation asked", typeof(UserInvitationDto))]
        [SwaggerResponse(500, "An error occured")]
        public IActionResult GetInvitation()
        {
            try
            {
                var entity = _service.Create(UserInvitation.GenerateNew());

                return Ok(entity.Dto());
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot generate an invitation, an unexpected error occured - {e}");
                return StatusCode(500, "Cannot register a new invitation into DB");
            }
        }

        #endregion
    }
}