using Api.Service;
using Api.Service.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Hosting.Dto
{
    [SwaggerSchema(Required = new[] { "Title", "Description" })]
    public class RecipeDto
    {
        [JsonProperty("id")]
        [SwaggerSchema("Unique identifier", ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("ownerId")]
        [SwaggerSchema("Owner identifier", ReadOnly = true)]
        public string OwnerUid { get; set; }

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
    }

    public static class RecipeDtoExtensions
    {
        public static Recipe Model(this RecipeDto recipe, List<Tag> existingTags)
        {
            var coucou = new Recipe
            {
                Id = recipe.Id,
                OwnerUid = recipe.OwnerUid,
                DateCreation = recipe.DateCreation,
                Title = recipe.Title,
                Description = recipe.Description,
                ImagePath = recipe.ImagePath,
                // we transform the Dto tags in Model tags (and if we find it in existing tags, we put that one to avoid duplicate keys)
                RecipeTags = recipe.Tags.Select(t => new RecipeTag
                {
                    Tag = existingTags.FirstOrDefault(e => e.Name == t.Name) ?? t.Model()
                }).ToList()
            };
            return coucou;
        }

        public static RecipeDto Dto(this Recipe entity)
        {
            return new RecipeDto
            {
                Id = entity.Id,
                OwnerUid = entity.OwnerUid,
                DateCreation = entity.DateCreation,
                Title = entity.Title,
                Description = entity.Description,
                ImagePath = entity.ImagePath,
                Tags = entity.RecipeTags.Select(rt => rt.Tag.Dto()).ToArray()
            };
        }
    }
}
