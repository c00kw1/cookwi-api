using Api.Service.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "RecipeId", "StepNumber", "Content" })]
    public class QuantityUnitDto : DtoObject
    {
        [JsonProperty("name")]
        [SwaggerSchema("Quantity unit name")]
        public string Name { get; set; }

        [JsonProperty("acronym")]
        [SwaggerSchema("Acronym such as m for meter or cm for centimeter")]
        public string Acronym { get; set; }

        [JsonProperty("type")]
        [EnumDataType(typeof(UnitType))]
        [JsonConverter(typeof(StringEnumConverter))]
        [SwaggerSchema("Unit type")]
        public UnitType Type { get; set; }
    }

    public static class QuantityUnitDtoExtensions
    {
        public static QuantityUnitDto Dto(this QuantityUnit qtyUnit)
        {
            return new QuantityUnitDto
            {
                Id = qtyUnit.Id,
                Name = qtyUnit.Name,
                Acronym = qtyUnit.Acronym,
                Type = qtyUnit.Type
            };
        }

        public static QuantityUnit Model(this QuantityUnitDto dto)
        {
            return new QuantityUnit
            {
                Name = dto.Name,
                Acronym = dto.Acronym,
                Type = dto.Type
            };
        }
    }
}
