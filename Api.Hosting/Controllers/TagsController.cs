using Api.Hosting.Dto;
using Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class TagsController : Controller
    {
        private CookwiDbContext _context;

        public TagsController(CookwiDbContext dbContext)
        {
            _context = dbContext;
        }

        /// <summary>
        /// Get all available tags globally
        /// </summary>
        /// <returns>List of tags</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Get all available tags globally")]
        [SwaggerResponse(200, "All available tags", typeof(TagDto[]))]
        [SwaggerResponse(400, "Cannot retrieve tags")]
        public ActionResult<TagDto[]> GetAllTags()
        {
            try
            {
                return _context.Tags.Select(t => t.Dto()).ToArray();
            }
            catch
            {
                return BadRequest("Cannot retrieve the list of tags");
            }
        }

        /// <summary>
        /// Get all current user's tags based on his recipes
        /// </summary>
        /// <returns>List of tags</returns>
        [HttpGet]
        [Route("my")]
        [SwaggerOperation(Summary = "Get all current user's tags based on his recipes")]
        [SwaggerResponse(200, "Current user all exsting tags", typeof(TagDto[]))]
        [SwaggerResponse(400, "Cannot retrieve tags")]
        public ActionResult<TagDto[]> GetMyTags()
        {
            try
            {
                return NotFound("Not implemented route");
            }
            catch
            {
                return BadRequest("Cannot retrieve the list of tags");
            }
        }

        /// <summary>
        /// Add a recipe tag if not already present
        /// </summary>
        /// <param name="tag">Tag to insert</param>
        /// <returns>Inserted tag</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Add a recipe tag if not already present")]
        [SwaggerResponse(201, "Recipe object created and inserted in db", typeof(TagDto))]
        [SwaggerResponse(400, "Cannot add the new tag, it may already exist")]
        public ActionResult<TagDto> PostTag([FromBody] TagDto tag)
        {
            try
            {
                var added = _context.Tags.Add(tag.Model());
                _context.SaveChanges();
                return Created(HttpContext.Request.Path, added.Entity.Dto());
            }
            catch
            {
                return BadRequest("The tag cannot be added, it may already exist");
            }
        }
    }
}