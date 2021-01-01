using Api.Hosting.Utils;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "Title", "Description" })]
    public class RecipeDto : DtoObject
    {
        [JsonProperty("ownerId")]
        [SwaggerSchema("Owner identifier", ReadOnly = true)]
        public string OwnerId { get; set; }

        [JsonProperty("dateCreation")]
        [SwaggerSchema("Date of creation of the recipe", Format = "date", ReadOnly = true)]
        public DateTime DateCreation { get; set; }

        [JsonProperty("title")]
        [SwaggerSchema("Title of the recipe")]
        public string Title { get; set; }

        [JsonProperty("description")]
        [SwaggerSchema("Description of the recipe")]
        public string Description { get; set; }

        [JsonProperty("cookingTime")]
        [SwaggerSchema("Cooking time (HH:mm)")]
        public TimeSpan CookingTime { get; set; }

        [JsonProperty("bakingTime")]
        [SwaggerSchema("Baking time (HH:mm)")]
        public TimeSpan BakingTime { get; set; }

        [JsonProperty("imagePath")]
        [SwaggerSchema("Path of the main recipe's image", ReadOnly = true)]
        public string ImagePath { get; set; }

        [JsonProperty("tags")]
        [SwaggerSchema("Tags of the recipe")]
        public string[] Tags { get; set; }

        [JsonProperty("steps")]
        [SwaggerSchema("Steps of the recipe")]
        public RecipeStepDto[] Steps { get; set; }

        [JsonProperty("ingredients")]
        [SwaggerSchema("Ingredients of the recipe")]
        public RecipeIngredientDto[] Ingredients { get; set; }
    }

    public static class RecipeDtoExtensions
    {
        public static RecipeDto Dto(this Recipe entity, S3 s3)
        {
            return new RecipeDto
            {
                Id = entity.Id,
                OwnerId = entity.OwnerId,
                Title = entity.Title,
                Description = entity.Description,
                CookingTime = entity.CookingTime,
                BakingTime = entity.BakingTime,
                ImagePath = s3.GetPresignedUrl(entity.ImagePath).GetAwaiter().GetResult(),
                Tags = entity.Tags.ToArray(),
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
                CookingTime = recipe.CookingTime,
                BakingTime = recipe.BakingTime,
                ImagePath = "",
                Tags = recipe.Tags.ToHashSet()
            };
            // we set the steps and the ingredients
            newRecipe.Steps = recipe.Steps.Select(step => step.Model()).ToList();
            newRecipe.Ingredients = recipe.Ingredients.Select(ingredient => ingredient.Model()).ToList();

            return newRecipe;
        }
    }
}
