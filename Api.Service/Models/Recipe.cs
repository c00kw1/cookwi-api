using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models
{
    [Table("rcp")]
    public class Recipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("ownuid")]
        public string OwnerUid { get; set; }

        [Column("datadd")]
        public DateTime DateCreation { get; set; }

        [Column("rcptit")]
        public string Title { get; set; }

        [Column("rcpdsc")]
        public string Description { get; set; }

        [Column("rcpimg")]
        public string ImagePath { get; set; }

        public ICollection<RecipeTag> RecipeTags { get; set; }
    }
}
