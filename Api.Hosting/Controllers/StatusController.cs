using Microsoft.AspNetCore.Mvc;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public ActionResult<string> Ping()
        {
            return Ok("pong");
        }
    }
}
