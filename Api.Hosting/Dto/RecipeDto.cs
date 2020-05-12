using Api.Hosting.Helpers;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "Title", "Description" })]
    public class RecipeDto
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(GuidConverter))]
        [SwaggerSchema("Unique identifier", ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("ownerId")]
        [SwaggerSchema("Owner identifier", ReadOnly = true)]
        public Guid OwnerId { get; set; }

        [JsonProperty("dateCreation")]
        [SwaggerSchema("Date of creation of the recipe", Format = "date", ReadOnly = true)]
        public DateTime DateCreation { get; set; }

        [JsonProperty("title")]
        [SwaggerSchema("Title of the recipe")]
        public string Title { get; set; }

        [JsonProperty("description")]
        [SwaggerSchema("Description of the recipe")]
        public string Description { get; set; }

        [JsonProperty("imagePath")]
        [SwaggerSchema("Path of the main recipe's image")]
        public string ImagePath { get; set; }

        [JsonProperty("tags")]
        [SwaggerSchema("Tags of the recipe")]
        public TagDto[] Tags { get; set; }

        [JsonProperty("steps")]
        [SwaggerSchema("Steps of the recipe")]
        public RecipeStepDto[] Steps { get; set; }

        [JsonProperty("ingredients")]
        [SwaggerSchema("Ingredients of the recipe")]
        public RecipeIngredientDto[] Ingredients { get; set; }
    }

    public static class RecipeDtoExtensions
    {
        public static RecipeDto Dto(this Recipe entity)
        {
            return new RecipeDto
            {
                Id = entity.Id,
                OwnerId = entity.OwnerId,
                DateCreation = entity.DateCreation,
                Title = entity.Title,
                Description = entity.Description,
                ImagePath = entity.ImagePath,
                Tags = entity.TagsLink.Select(rt => rt.Tag.Dto()).ToArray(),
                Steps = entity.Steps.Select(s => s.Dto()).ToArray(),
                Ingredients = entity.Ingredients.Select(s => s.Dto()).ToArray()
            };
        }

        public static Recipe Model(this RecipeDto recipe)
        {
            var newRecipe = new Recipe
            {
                Title = recipe.Title,
                Description = recipe.Description,
                ImagePath = recipe.ImagePath
            };
            // we set the tags
            newRecipe.TagsLink = recipe.Tags.Select(newTag =>
            {
                return new RecipeTag
                {
                    Recipe = newRecipe,
                    Tag = newTag.Model()
                };
            }).ToList();
            // we set the steps and the ingredients
            newRecipe.Steps = recipe.Steps.Select(step => step.Model()).ToList();
            newRecipe.Ingredients = recipe.Ingredients.Select(ingredient => ingredient.Model()).ToList();

            return newRecipe;
        }
    }
}
