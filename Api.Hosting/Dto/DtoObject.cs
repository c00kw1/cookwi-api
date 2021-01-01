using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Hosting.Dto
{
    public abstract class DtoObject
    {
        [JsonProperty("id")]
        [SwaggerSchema("Unique identifier", ReadOnly = true)]
        public string Id { get; set; }
    }
}
