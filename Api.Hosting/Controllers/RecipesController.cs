using Api.Hosting.Dto;
using Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;

namespace Api.Hosting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class RecipesController : Controller
    {
        private CookwiDbContext _context;

        public RecipesController(CookwiDbContext dbContext)
        {
            _context = dbContext;
        }

        /// <summary>
        /// Retrieve all the recipes for the current user
        /// </summary>
        /// <returns>List of recipes</returns>
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve all the recipes for the current user")]
        [SwaggerResponse(200, "List of all the recipes for current authenticated user", typeof(RecipeDto[]))]
        public ActionResult<RecipeDto[]> GetAllRecipes()
        {
            var all = _context.Recipes
                              .Include(r => r.RecipeTags)
                              .ThenInclude(rt => rt.Tag)
                              .Select(rt => rt.Dto());
            return Ok(all);
        }

        /// <summary>
        /// Recipe object created and inserted in db
        /// </summary>
        /// <param name="recipe">Recipe to insert</param>
        /// <returns>Inserted recipe</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Add a new recipe")]
        [SwaggerResponse(201, "Recipe object created and inserted in db", typeof(RecipeDto))]
        [SwaggerResponse(400, "Cannot add the new recipe as an error occured")]
        public ActionResult<RecipeDto> PostRecipe([FromBody] RecipeDto recipe)
        {
            try
            {
                recipe.DateCreation = DateTime.Now;
                recipe.OwnerUid = "auth0|4242"; // TODO : will be the real user id from auth0

                // 1_ we collect tags chosen for the new recipe strings
                // 2_ we collect tags existing in db for those recipe tags we want
                // 3_ we add the recipe telling the Model convertion we found some existing tags already to avoid duplicate keys
                var recipeTags = recipe.Tags.Select(t => t.Name).Distinct().ToHashSet();
                var existingTags = _context.Tags.Where(t => recipeTags.Contains(t.Name)).ToList();
                var added = _context.Recipes.Add(recipe.Model(existingTags));
                _context.SaveChanges();
                return Created(HttpContext.Request.Path, added.Entity.Dto());
            }
            catch
            {
                return BadRequest("The recipe cannot be added");
            }
        }
    }
}