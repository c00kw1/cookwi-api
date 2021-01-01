using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatusController : Controller
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Check if API is alive")]
        [SwaggerResponse(200, "Simple pong string", typeof(string))]
        [SwaggerResponse(500, "An unexpected error has occured")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
