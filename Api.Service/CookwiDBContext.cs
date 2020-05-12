using Api.Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Service
{
    public class CookwiDbContext : DbContext
    {
        public CookwiDbContext(DbContextOptions<CookwiDbContext> options) : base(options)
        { }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeIngredientUnit> RecipeIngredientUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("uuid-ossp");

            // Recipe -> Steps
            builder.Entity<Recipe>().HasMany(recipe => recipe.Steps)
                                    .WithOne(step => step.Recipe)
                                    .HasForeignKey(step => step.RecipeId)
                                    .IsRequired()
                                    .OnDelete(DeleteBehavior.Cascade);

            // Recipe -> Ingredients
            builder.Entity<Recipe>().HasMany(recipe => recipe.Ingredients)
                                    .WithOne(ingredient => ingredient.Recipe)
                                    .HasForeignKey(ingredient => ingredient.RecipeId)
                                    .IsRequired()
                                    .OnDelete(DeleteBehavior.Cascade);

            // Tag unique key
            builder.Entity<Tag>().HasIndex(e => e.Name).IsUnique();

            // RecipeTag -> Recipe / Tag
            builder.Entity<RecipeTag>().HasKey(rt => new { rt.RecipeId, rt.TagId });
            builder.Entity<RecipeTag>().HasOne(rt => rt.Recipe).WithMany(r => r.TagsLink).HasForeignKey(rt => rt.RecipeId);
            builder.Entity<RecipeTag>().HasOne(rt => rt.Tag).WithMany(t => t.RecipesLink).HasForeignKey(rt => rt.TagId);

            // RecipeIngredient -> RecipeIngredientUnit
            builder.Entity<RecipeIngredient>().HasOne(ringredient => ringredient.Unit)
                                              .WithMany(riunit => riunit.RecipeIngredients)
                                              .HasForeignKey(ringredient => ringredient.UnitName)
                                              .IsRequired();
        }

        #region Recipes

        public async Task<Recipe[]> GetAllRecipes()
        {
            return await Recipes
                          .Include(r => r.Steps)
                          .Include(r => r.Ingredients).ThenInclude(i => i.Unit)
                          .Include(r => r.TagsLink).ThenInclude(rt => rt.Tag)
                          .ToArrayAsync();
        }

        public async Task<Recipe> GetOneRecipe(Guid id)
        {
            return await Recipes.Where(r => r.Id == id)
                          .Include(r => r.Steps)
                          .Include(r => r.Ingredients).ThenInclude(i => i.Unit)
                          .Include(r => r.TagsLink).ThenInclude(rt => rt.Tag)
                          .FirstOrDefaultAsync();
        }

        #endregion

        #region Tags

        public async Task<Tag> GetOrCreateTag(Tag askedTag)
        {
            var existingTag = await Tags.Where(t => t.Name == askedTag.Name).FirstOrDefaultAsync();
            if (existingTag == null)
            {
                var addedTag = await Tags.AddAsync(askedTag);
                existingTag = addedTag.Entity;
            }
            return existingTag;
        }

        #endregion
    }
}
