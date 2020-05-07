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
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }

    public class GuidConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Guid) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return Guid.Empty;
                case JsonToken.String:
                    string str = reader.Value as string;
                    if (string.IsNullOrEmpty(str))
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        return new Guid(str);
                    }
                default:
                    throw new ArgumentException("Invalid token type");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (Guid.Empty.Equals(value))
            {
                writer.WriteValue("");
            }
            else
            {
                writer.WriteValue((Guid)value);
            }
        }
    }
}
