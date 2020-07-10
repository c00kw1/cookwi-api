﻿using Api.Service.Models;
using Api.Service.Models.Admin;
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

        public DbSet<User> Users { get; set; }
        public DbSet<UserInvitation> UserInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("uuid-ossp");

            #region Recipes and tags

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

            #endregion
        }

        #region Recipes

        public async Task<Recipe[]> GetAllRecipes(Guid userId)
        {
            return await Recipes.Where(r => r.OwnerId == userId)
                          .Include(r => r.Steps)
                          .Include(r => r.Ingredients).ThenInclude(i => i.Unit)
                          .Include(r => r.TagsLink).ThenInclude(rt => rt.Tag)
                          .ToArrayAsync();
        }

        public async Task<Recipe> GetOneRecipe(Guid id, Guid userId)
        {
            return await Recipes.Where(r => r.Id == id && r.OwnerId == userId)
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

        public async Task<Tag[]> GetAllMyTags(Guid userId)
        {
            var recipes = await GetAllRecipes(userId);
            return recipes.SelectMany(r => r.TagsLink.Select(t => t.Tag)).Distinct().ToArray();
        }

        #endregion
    }
}
