using Api.Hosting.Helpers;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "RecipeId", "StepNumber", "Content" })]
    public class RecipeIngredientDto
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(GuidConverter))]
        [SwaggerSchema("Unique identiifier", ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("recipeId")]
        [SwaggerSchema("Recipe id the step belongs to", ReadOnly = true)]
        public Guid RecipeId { get; set; }

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
        public static RecipeIngredientDto Dto(this RecipeIngredient step)
        {
            return new RecipeIngredientDto
            {
                Id = step.Id,
                RecipeId = step.RecipeId,
                Name = step.Name,
                Quantity = step.Quantity,
                Unit = step.Unit.Name,
            };
        }

        public static RecipeIngredient Model(this RecipeIngredientDto entity)
        {
            return new RecipeIngredient
            {
                Name = entity.Name,
                Quantity = entity.Quantity,
                UnitName = entity.Unit
            };
        }
    }
}
