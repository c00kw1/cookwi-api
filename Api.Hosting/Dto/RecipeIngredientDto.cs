using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Hosting.Dto
{
    public class RecipeIngredientDto
    {
        [JsonProperty("name")]
        [SwaggerSchema("Ingredient name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        [SwaggerSchema("Ingredient quantity")]
        public double Quantity { get; set; }

        [JsonProperty("unit")]
        [SwaggerSchema("Ingredient's quantity unit")]
        public string Unit { get; set; }
    }

    public static class RecipeIngredientDtoExtensions
    {
        public static RecipeIngredientDto Dto(this RecipeIngredientMongo step)
        {
            return new RecipeIngredientDto
            {
                Name = step.Name,
                Quantity = step.Quantity,
                Unit = step.Unit,
            };
        }

        public static RecipeIngredientMongo Model(this RecipeIngredientDto entity)
        {
            return new RecipeIngredientMongo
            {
                Name = entity.Name,
                Quantity = entity.Quantity,
                Unit = entity.Unit
            };
        }
    }
}