using Api.Hosting.Helpers;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "Name" })]
    public class TagDto
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(GuidConverter))]
        [SwaggerSchema("Tag unique identiifier", ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        [SwaggerSchema("Tag name")]
        public string Name { get; set; }
    }

    public static class TagDtoExtensions
    {
        public static TagDto Dto(this Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }

        public static Tag Model(this TagDto entity)
        {
            return new Tag
            {
                Name = entity.Name
            };
        }
    }
}
