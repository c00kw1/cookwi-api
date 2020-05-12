using Api.Hosting.Controllers;
using Api.Hosting.Dto;
using Api.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;

namespace Api.Tests
{
    public class RecipeEntityTests
    {
        private DbContextOptions<CookwiDbContext> _options;
        private CookwiDbContext _context;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _options = new DbContextOptionsBuilder<CookwiDbContext>().UseSqlite("DataSource=:memory:").Options;
            _context = new CookwiDbContext(_options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _context?.Database.CloseConnection();
            _context?.Dispose();
        }

        [Test]
        public void EFCore_sould_recognize_tag_already_in_db_when_creating_recipes()
        {
            // this is what we get from API route POST
            var recipe = new RecipeDto
            {
                Title = "test",
                Description = "test",
                ImagePath = "test",
                Tags = new[] { new TagDto { Name = "tagtest" } },
                Steps = new[] { new RecipeStepDto { Content = "steptest", StepNumber = 1 } }
            };
            var controller = new RecipesController(_context);

            var result = controller.PostRecipe(recipe).GetAwaiter().GetResult();

            Assert.That(result, Is.Not.Null);

            var result2 = controller.PostRecipe(recipe).GetAwaiter().GetResult();

            Assert.That(result2, Is.Not.Null);

            var tagsList = _context.Tags.ToList();

            Assert.That(tagsList, Has.Count.EqualTo(1), "We added 2 times the same tag, we should only have 1 registered since there is a Unique constraint on the name");
        }
    }
}
