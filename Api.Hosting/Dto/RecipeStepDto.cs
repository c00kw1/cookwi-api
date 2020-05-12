using Api.Hosting.Helpers;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "RecipeId", "StepNumber", "Content" })]
    public class RecipeStepDto
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(GuidConverter))]
        [SwaggerSchema("Unique identiifier", ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("recipeId")]
        [SwaggerSchema("Recipe id the step belongs to", ReadOnly = true)]
        public Guid RecipeId { get; set; }

        [JsonProperty("stepNumber")]
        [SwaggerSchema("Recipe id the step belongs to")]
        public int StepNumber { get; set; }

        [JsonProperty("content")]
        [SwaggerSchema("Recipe id the step belongs to")]
        public string Content { get; set; }
    }

    public static class RecipeStepDtoExtensions
    {
        public static RecipeStepDto Dto(this RecipeStep step)
        {
            return new RecipeStepDto
            {
                Id = step.Id,
                RecipeId = step.RecipeId,
                StepNumber = step.StepNumber,
                Content = step.Content
            };
        }

        public static RecipeStep Model(this RecipeStepDto entity)
        {
            return new RecipeStep
            {
                Id = entity.Id,
                RecipeId = entity.RecipeId,
                StepNumber = entity.StepNumber,
                Content = entity.Content
            };
        }
    }
}
