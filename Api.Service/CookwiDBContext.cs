using Api.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Service
{
    public class CookwiDbContext : DbContext
    {
        public CookwiDbContext(DbContextOptions<CookwiDbContext> options) : base(options)
        { }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("uuid-ossp");

            builder.Entity<Tag>().HasIndex(e => e.Name).IsUnique();
            // composite primary key for RecipeTag
            builder.Entity<RecipeTag>().HasKey(rt => new { rt.RecipeId, rt.TagId });
            builder.Entity<RecipeTag>().HasOne(rt => rt.Recipe).WithMany(r => r.RecipeTags).HasForeignKey(rt => rt.RecipeId).IsRequired();
        }
    }
}
