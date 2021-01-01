using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Hosting.Dto
{
    public class RecipeStepDto
    {
        [JsonProperty("position")]
        [SwaggerSchema("Recipe id the step belongs to")]
        public int Position { get; set; }

        [JsonProperty("content")]
        [SwaggerSchema("Recipe id the step belongs to")]
        public string Content { get; set; }
    }

    public static class RecipeStepDtoExtensions
    {
        public static RecipeStepDto Dto(this RecipeStepMongo step)
        {
            return new RecipeStepDto
            {
                Position = step.Position,
                Content = step.Content
            };
        }

        public static RecipeStepMongo Model(this RecipeStepDto entity)
        {
            return new RecipeStepMongo
            {
                Position = entity.Position,
                Content = entity.Content
            };
        }
    }
}