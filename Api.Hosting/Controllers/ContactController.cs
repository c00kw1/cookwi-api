using Api.Hosting.Dto;
using Api.Hosting.Helpers;
using Api.Hosting.Settings;
using Api.Service.Mongo;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContactController : Controller
    {
        private ContactService _service;
        private RecaptchaSettings _captchaSettings;

        public ContactController(ContactService contactService, RecaptchaSettings captchaSettings)
        {
            _service = contactService;
            _captchaSettings = captchaSettings;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Send a message to website administrators")]
        [SwaggerResponse(201, "Message sent", typeof(ContactDto))]
        [SwaggerResponse(500, "An unexpected error has occured")]
        public async Task<IActionResult> PostMessage([FromBody] ContactDto message)
        {
            var validator = new ContactDtoValidator();
            var result = validator.Validate(message);

            if (!result.IsValid)
            {
                return BadRequest(result.ToHttpError("Bad request, some fields are not good"));
            }

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "secret", _captchaSettings.ServerKey },
                    { "response", message.Token }
                };
                // we must validate the token now
                using (var client = new HttpClient())
                using (var str = new FormUrlEncodedContent(parameters))
                {
                    client.BaseAddress = new Uri("https://www.google.com");
                    var captchaResponse = await client.PostAsync("/recaptcha/api/siteverify", str);
                    var response = JsonConvert.DeserializeObject<CaptchaResponse>(await captchaResponse.Content.ReadAsStringAsync());
                    if (!response.Success)
                    {
                        return StatusCode(401, new HttpErrorDto("reCaptcha did not confirm your request"));
                    }
                }
                var entity = message.Model();
                var created = _service.Create(entity);
                return Created(HttpContext?.Request.Path.Value ?? "", created.Dto());
            }
            catch(Exception e)
            {
                return StatusCode(500, new HttpErrorDto());
            }
        }
    }

    public class CaptchaRequest
    {
        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }
    }

    public class CaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }
    }
}
