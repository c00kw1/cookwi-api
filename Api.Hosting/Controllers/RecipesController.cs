using Api.Hosting.Dto;
using Api.Hosting.Helpers;
using Api.Hosting.Utils;
using Api.Service;
using Api.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class RecipesController : Controller
    {
        private CookwiDbContext _context;
        private S3 _s3;

        public RecipesController(CookwiDbContext dbContext, S3 s3)
        {
            _context = dbContext;
            _s3 = s3;
        }

        /// <summary>
        /// Retrieve all the recipes for the current user
        /// </summary>
        /// <returns>List of recipes</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve all the recipes for the current user")]
        [SwaggerResponse(200, "List of all the recipes for current authenticated user", typeof(RecipeDto[]))]
        public async Task<IActionResult> GetAllRecipes()
        {
            var userId = UserHelper.GetId(HttpContext.User);
            var all = await _context.GetAllRecipes(userId);
            return Ok(all.Select(r => r.Dto(_s3)).ToArray());
        }

        /// <summary>
        /// Retrieve a recipe from its ID
        /// </summary>
        /// <returns>One Recipe</returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Retrieve one recipe by its ID")]
        [SwaggerResponse(200, "Asked recipe", typeof(RecipeDto))]
        [SwaggerResponse(404, "Recipe not fount")]
        public async Task<IActionResult> GetRecipeById(Guid id)
        {
            var userId = UserHelper.GetId(HttpContext.User);
            var recipe = await _context.GetOneRecipe(id, userId);

            if (recipe == null)
            {
                return NotFound();
            }
            return Ok(recipe.Dto(_s3));
        }

        /// <summary>
        /// Add a new recipe
        /// </summary>
        /// <param name="recipe">Recipe to insert</param>
        /// <returns>Inserted recipe</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Add a new recipe")]
        [SwaggerResponse(201, "Recipe object created and inserted in db", typeof(RecipeDto))]
        [SwaggerResponse(400, "At least one of recipe's field is wrong", typeof(string))]
        [SwaggerResponse(500, "Cannot add the new recipe as an error occured")]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeDto recipe)
        {
            try
            {
                var recipeToAdd = recipe.Model();
                recipeToAdd.DateCreation = DateTime.Now;
                recipeToAdd.OwnerId = UserHelper.GetId(HttpContext.User);
                recipeToAdd.TagsLink = await FilterExistingTags(recipeToAdd.TagsLink.ToList());
                recipeToAdd.Ingredients = CheckIngredientsUnits(recipeToAdd.Ingredients.ToList(), out var badUnits);

                if (badUnits.Any())
                {
                    return BadRequest($"Some quantity units used in ingredients are unknown, please chose among the ones provided. Errors on : {string.Join(';', badUnits)}");
                }
                
                var added = await _context.Recipes.AddAsync(recipeToAdd);
                await _context.SaveChangesAsync();
                return Created(HttpContext?.Request.Path.Value ?? "", added.Entity.Dto(_s3));
            }
            catch
            {
                return StatusCode(500, "The recipe cannot be added");
            }
        }

        /// <summary>
        /// Update an existing recipe
        /// </summary>
        /// <param name="id">Recipe id</param>
        /// <param name="updatedRecipe">Updated recipe</param>
        /// <returns>Inserted recipe</returns>
        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Update an existing recipe")]
        [SwaggerResponse(201, "Recipe object updated in db", typeof(RecipeDto))]
        [SwaggerResponse(400, "At least one of recipe's field is wrong", typeof(string[]))]
        [SwaggerResponse(404, "Recipe to update is not found / does not exist")]
        [SwaggerResponse(500, "Cannot update the recipe as an error occured")]
        public async Task<IActionResult> UpdateRecipe(Guid id, [FromBody]RecipeDto updatedRecipe)
        {
            try
            {
                var userId = UserHelper.GetId(HttpContext.User);
                var existingRecipe = await _context.GetOneRecipe(id, userId);
                if (existingRecipe == null)
                {
                    return NotFound($"Recipe {id} cannot be found on this account");
                }

                var errors = await UpdateRecipe(updatedRecipe, existingRecipe);
                if (errors.Any())
                {
                    return BadRequest(errors);
                }

                await _context.SaveChangesAsync();
                return Created(HttpContext.Request.Path, existingRecipe.Dto(_s3));
            }
            catch
            {
                return StatusCode(500, "The recipe cannot be updated");
            }
        }

        [HttpPost]
        [Route("{id}/image")]
        [SwaggerOperation("Adds an image to a recipe")]
        [SwaggerResponse(200, "Image added")]
        [SwaggerResponse(400, "Image format is wrong")]
        [SwaggerResponse(403, "This recipe does not belong to the user")]
        [SwaggerResponse(500, "An error occured with the server (s3 ?)")]
        public async Task<IActionResult> AddImage(Guid id, IFormFile file)
        {
            var authorizedFormats = new[] { "image/png", "image/jpeg" };
            var minWidth = 800;
            var minHeight = 600;
            var maxSize = 10.Mb();

            var recipe = await _context.GetOneRecipe(id, UserHelper.GetId(HttpContext.User));
            if (recipe == null)
            {
                return StatusCode(403, $"Recipe {id} does not belong to you");
            }

            if (file == null)
            {
                return BadRequest("No image sent");
            }
            if (!authorizedFormats.Contains(file.ContentType.ToLower()))
            {
                return BadRequest($"Bad image format, authorized are {string.Join(", ", authorizedFormats)}");
            }

            using (var image = Image.FromStream(file.OpenReadStream()))
            {
                if (image.Width < 800 || image.Height < 600 || file.Length > maxSize)
                {
                    return BadRequest($"Image must be at least {minWidth}x{minHeight} pixels and less than {maxSize}Mo");
                }
            }

            var name = $"recipes/{id}/cover{Path.GetExtension(file.FileName)}";
            await _s3.AddFile(name, file);

            recipe.ImagePath = name;
            _context.SaveChanges();

            return Ok();
        }

        #region Ingredients
        
        [HttpGet]
        [Route("quantity-units")]
        [SwaggerOperation(Summary = "Get all the available quantity units")]
        [SwaggerResponse(200, "Quantity units available", typeof(string[]))]
        [SwaggerResponse(500, "An error occured while gathering quantity units")]
        public async Task<IActionResult> GetAllQuantityUnits()
        {
            try
            {
                return Ok(await _context.RecipeIngredientUnits.Select(riu => riu.Name).ToArrayAsync());
            }
            catch
            {
                return StatusCode(500, "Cannot retrieve the list of quantity units, an error occured");
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Find existing tags out of <paramref name="givenRTags"/> and prepare a List for being transfered to EFCore
        /// </summary>
        /// <param name="givenRTags"></param>
        /// <returns>Prepared list of RecipeTag</returns>
        private async Task<List<RecipeTag>> FilterExistingTags(List<RecipeTag> givenRTags)
        {
            var toAddTagsNames = givenRTags.Select(t => t.Tag.Name).ToHashSet();
            // we get the existing tags
            var existingTags = await _context.Tags.Where(t => toAddTagsNames.Contains(t.Name)).ToListAsync();
            // for each tag added by the user to its recipe
            givenRTags.ForEach(rtag =>
            {
                var searchTag = existingTags.FirstOrDefault(t => t.Name == rtag.Tag.Name);
                if (searchTag != null)
                {
                    rtag.Tag = searchTag;
                    rtag.TagId = searchTag.Id;
                }
            });

            return givenRTags;
        }

        private List<RecipeIngredient> CheckIngredientsUnits(List<RecipeIngredient> givenIngredients, out List<string> badUnits)
        {
            var availableUnits = _context.RecipeIngredientUnits.ToHashSet();
            var notFoundUnits = new List<string>();

            // for each ingredient the user wants to add we gonna check the unit in DB
            givenIngredients.ForEach(ingredient =>
            {
                var foundUnit = availableUnits.FirstOrDefault(existing => existing.Name == ingredient.UnitName);
                if (foundUnit == null)
                {
                    // unit used is unknown, which is an error, we don't want users to do whatever they want with units
                    notFoundUnits.Add(ingredient.UnitName);
                    return;
                }
                // if we found one in DB, we set the 
                ingredient.Unit = foundUnit;
                ingredient.UnitName = foundUnit.Name;
            });

            badUnits = notFoundUnits;
            return givenIngredients;
        }

        /// <summary>
        /// Will update <paramref name="existing"/> with <paramref name="updated"/> values
        /// </summary>
        /// <param name="updated">Received from API</param>
        /// <param name="existing">DB existing object</param>
        private async Task<List<string>> UpdateRecipe(RecipeDto updated, Recipe existing)
        {
            var errors = new List<string>();

            _context.Entry(existing).State = EntityState.Modified;
            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.ImagePath = updated.ImagePath;

            //remove all tags links to add new ones from updated entity
            _context.RecipeTags.RemoveRange(existing.TagsLink);
            existing.TagsLink = await FilterExistingTags(updated.Model().TagsLink.ToList());

            // remove all current steps to add new ones from updated entity
            _context.RecipeSteps.RemoveRange(existing.Steps);
            existing.Steps = updated.Steps.Select(step => step.Model()).ToList();

            // remove all current ingredients to add new ones from updated entity
            _context.RecipeIngredients.RemoveRange(existing.Ingredients);
            existing.Ingredients = CheckIngredientsUnits(updated.Model().Ingredients.ToList(), out var badUnits);
            if (badUnits.Any()) errors.Add($"Usage of unknown quantity units: {string.Join(';', badUnits)}");

            return errors;
        }

        #endregion
    }
}