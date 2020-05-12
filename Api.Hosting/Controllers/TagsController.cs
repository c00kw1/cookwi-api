using Api.Hosting.Dto;
using Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

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
        [SwaggerResponse(500, "Cannot retrieve tags, there was an error")]
        public async Task<IActionResult> GetAllTags()
        {
            try
            {
                return Ok(await _context.Tags.Select(t => t.Dto()).ToArrayAsync());
            }
            catch
            {
                return StatusCode(500, "Cannot retrieve the list of tags, an error occured");
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
        [SwaggerResponse(500, "Cannot retrieve tags")]
        public async Task<IActionResult> GetMyTags()
        {
            try
            {
                return NotFound("Not implemented route");
            }
            catch
            {
                return StatusCode(500, "Cannot retrieve the list of tags");
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
        [SwaggerResponse(500, "Cannot add the new tag, it may already exist")]
        public async Task<IActionResult> PostTag([FromBody] TagDto tag)
        {
            try
            {
                var added = await _context.Tags.AddAsync(tag.Model());
                await _context.SaveChangesAsync();
                return Created(HttpContext.Request.Path, added.Entity.Dto());
            }
            catch
            {
                return StatusCode(500, "The tag cannot be added, it may already exist");
            }
        }
    }
}