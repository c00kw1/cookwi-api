using Newtonsoft.Json;
using System.Collections.Generic;

namespace Api.Hosting.Dto
{
    public class HttpErrorDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("fields")]
        public List<HttpFieldErrorDto> Fields { get; set; }

        public HttpErrorDto(string message = "An error has occured", List<HttpFieldErrorDto> fieldsErrors = null)
        {
            Message = message;
            Fields = fieldsErrors ?? new List<HttpFieldErrorDto>();
        }
    }

    public class HttpFieldErrorDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        public HttpFieldErrorDto(string name, string error)
        {
            Name = name;
            Error = error;
        }
    }
}
