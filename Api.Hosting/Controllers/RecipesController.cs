using Api.Hosting.Dto;
using Api.Hosting.Helpers;
using Api.Hosting.Utils;
using Api.Service.Exceptions;
using Api.Service.Models;
using Api.Service.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class RecipesController : Controller
    {
        private ILogger _logger;

        private S3 _s3;
        private RecipesService _recipesService;
        private QuantityUnitsService _unitsService;

        public RecipesController(S3 s3, RecipesService recipesService, QuantityUnitsService unitsService, ILogger<RecipesController> logger)
        {
            _s3 = s3;
            _recipesService = recipesService;
            _unitsService = unitsService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all the recipes for the current user
        /// </summary>
        /// <returns>List of recipes</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve all the recipes for the authenticated user")]
        [SwaggerResponse(200, "List of all the recipes for current authenticated user", typeof(RecipeDto[]))]
        [SwaggerResponse(404, "Resource not found")]
        [SwaggerResponse(500, "Internal server error")]
        public IActionResult GetAllRecipes()
        {
            try
            {
                var userId = UserHelper.GetId(HttpContext.User);
                var all = _recipesService.GetAll(userId);

                return Ok(all.Select(r => r.Dto(_s3)).ToArray());
            }
            catch (NotFoundException e)
            {
                _logger.LogInformation(e.Message);
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get all recipes : {e}");
                return StatusCode(500, "Cannot get the list of recipes, an unexpected error has occured.");
            }
        }

        /// <summary>
        /// Retrieve a recipe from its ID
        /// </summary>
        /// <returns>One Recipe</returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Retrieve one recipe by its ID")]
        [SwaggerResponse(200, "Asked recipe", typeof(RecipeDto))]
        [SwaggerResponse(400, "Bad id format")]
        [SwaggerResponse(404, "Recipe not found")]
        [SwaggerResponse(500, "Internal server error")]
        public IActionResult GetRecipeById(string id)
        {
            try
            {
                var userId = UserHelper.GetId(HttpContext.User);
                var recipe = _recipesService.GetOne(id, userId);

                if (recipe == null)
                {
                    return NotFound();
                }

                return Ok(recipe.Dto(_s3));
            }
            catch (FormatException e)
            {
                return BadRequest("Bad id format");
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get the recipe by id {id} - {e}");
                return StatusCode(500, "Cannot get the recipe, an unexpected error has occured");
            }
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
        public IActionResult CreateRecipe([FromBody] RecipeDto recipe)
        {
            try
            {
                var recipeToAdd = recipe.Model();
                recipeToAdd.OwnerId = UserHelper.GetId(HttpContext.User);
                recipeToAdd.Ingredients = CheckIngredientsUnits(recipeToAdd.Ingredients.ToList(), out var badUnits);

                if (badUnits.Any())
                {
                    return BadRequest($"Some quantity units used in ingredients are unknown, please chose among the ones provided. Errors on : {string.Join(';', badUnits)}");
                }

                var added = _recipesService.Create(recipeToAdd);
                return Created(HttpContext?.Request.Path.Value ?? "", added.Dto(_s3));
            }
            catch (Exception e)
            {
                _logger.LogError($"Recipe cannot be added - {e}");
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
        [SwaggerResponse(200, "Recipe object updated in db", typeof(RecipeDto))]
        [SwaggerResponse(400, "At least one of recipe's field is wrong", typeof(string[]))]
        [SwaggerResponse(404, "Recipe to update is not found / does not exist")]
        [SwaggerResponse(500, "Cannot update the recipe as an error occured")]
        public IActionResult UpdateRecipe(string id, [FromBody]RecipeDto updatedRecipe)
        {
            var userId = UserHelper.GetId(HttpContext.User);

            try
            {
                var oldRecipe = _recipesService.GetOne(id, userId);
                if (oldRecipe == null)
                {
                    return NotFound();
                }
                var entity = updatedRecipe.Model();
                // re-write important informations
                entity.Id = id;
                entity.OwnerId = userId;
                entity.CreationDate = oldRecipe.CreationDate;
                entity.ImagePath = oldRecipe.ImagePath;
                var ingredients = CheckIngredientsUnits(entity.Ingredients, out var badUnits);
                if (badUnits != null && badUnits.Count > 0)
                {
                    return BadRequest("Bad ingredients units used.");
                }
                entity.Ingredients = ingredients;

                var result = _recipesService.Update(entity);
                if (!result) throw new Exception("Mongo result is not acknoledge. Unexpected error.");

                var returned = _recipesService.GetOne(entity.Id, userId);

                return Ok(returned.Dto(_s3));
            }
            catch (NotFoundException)
            {
                _logger.LogInformation($"Recipe to update {id} for user {userId} is not found");
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot update recipe {updatedRecipe.Id} for user {userId} - {e}");
                return StatusCode(500, "The recipe cannot be updated");
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation(Summary = "Removes an existing recipe")]
        [SwaggerResponse(200, "Recipe object removed from db")]
        [SwaggerResponse(404, "Recipe to remove is not found / does not exist")]
        [SwaggerResponse(500, "Cannot remove the recipe as an error occured")]
        public IActionResult DeleteRecipe(string id)
        {
            var userId = UserHelper.GetId(HttpContext.User);

            try
            {
                _recipesService.Remove(id, userId);
                return Ok();
            }
            catch (NotFoundException)
            {
                _logger.LogInformation($"Recipe to remove {id} for user {userId} is not found");
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot remove recipe {id} for user {userId} - {e}");
                return StatusCode(500, "The recipe cannot be removed");
            }
        }

        [HttpPost]
        [Route("{id}/image")]
        [RequestSizeLimit(10_485_760)] // 10 Mb
        [SwaggerOperation("Adds an image to a recipe")]
        [SwaggerResponse(200, "Image added")]
        [SwaggerResponse(400, "Image format is wrong")]
        [SwaggerResponse(404, "Recipe not found")]
        [SwaggerResponse(413, "File too large")]
        [SwaggerResponse(500, "An error occured with the server (s3 ?)")]
        public IActionResult AddImage(string id, IFormFile file)
        {
            var userId = UserHelper.GetId(HttpContext.User);
            var authorizedFormats = new[] { "image/png", "image/jpeg" };
            var minWidth = 800;
            var minHeight = 600;
            var maxSize = 10.Mb();

            try
            {
                var recipe = _recipesService.GetOne(id, userId);
                if (recipe == null)
                {
                    return NotFound();
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
                _s3.AddFile(name, file).GetAwaiter().GetResult();

                recipe.ImagePath = name;
                _recipesService.Update(recipe);

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot add image to recipe, internal error - {e}");
                return StatusCode(500, "Cannot add image to the recipe, an unexpected error has occured.");
            }
        }

        #region Ingredients
        
        [HttpGet]
        [Route("quantity-units")]
        [SwaggerOperation(Summary = "Get all the available quantity units")]
        [SwaggerResponse(200, "The units", typeof(QuantityUnitDto[]))]
        [SwaggerResponse(500, "An error has occured")]
        public IActionResult GetAll()
        {
            try
            {
                var units = _unitsService.GetAll().Select(e => e.Dto()).ToArray();
                return Ok(units);
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get all quantity units - {e}");
                return StatusCode(500, "An error has occured");
            }
        }

        #endregion

        #region Tags

        [HttpGet]
        [Route("tags")]
        [SwaggerOperation("Retrieve all tags used by current user's recipes")]
        [SwaggerResponse(200, "All tags", typeof(string[]))]
        [SwaggerResponse(500, "Unexpected error occured")]
        public IActionResult GetAllTags()
        {
            var userId = UserHelper.GetId(HttpContext.User);

            try
            {
                var tags = _recipesService.GetAllTags(userId);

                return Ok(tags);
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot load all tags, an unexpected error occured - {e}");
                return StatusCode(500, "Cannot retrieve all tags, an unexpected error occured");
            }
        }

        #endregion

        #region Utils

        private List<RecipeIngredientMongo> CheckIngredientsUnits(List<RecipeIngredientMongo> givenIngredients, out List<string> badUnits)
        {
            var availableUnits = _unitsService.GetAll().ToList();
            var notFoundUnits = new List<string>();

            // for each ingredient the user wants to add we gonna check the unit in Mongo
            givenIngredients.ForEach(ingredient =>
            {
                var foundUnit = availableUnits.FirstOrDefault(existing => existing.Acronym == ingredient.Unit);
                if (foundUnit == null)
                {
                    // unit used is unknown, which is an error, we don't want users to do whatever they want with units
                    notFoundUnits.Add(ingredient.Unit);
                    return;
                }
                ingredient.Unit = foundUnit.Acronym;
            });

            badUnits = notFoundUnits;
            return givenIngredients;
        }

        #endregion
    }
}