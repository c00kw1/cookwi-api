using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Api.Service.Models
{
    [Table("tag")]
    [Serializable]
    public class Tag : IEquatable<Tag>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tagnam")]
        public string Name { get; set; }

        public ICollection<RecipeTag> RecipesLink { get; set; }

        public bool Equals([AllowNull] Tag other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name == Name;
        }
    }
}
