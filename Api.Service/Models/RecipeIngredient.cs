using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models
{
    [Table("rcping")]
    [Serializable]
    public class RecipeIngredient
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("rcpid")]
        public Guid RecipeId { get; set; }

        [Column("nam")]
        public string Name { get; set; }

        [Column("qty")]
        public double Quantity { get; set; }

        [Column("qtyunt")]
        public string UnitName { get; set; }

        public Recipe Recipe { get; set; }
        public RecipeIngredientUnit Unit { get; set; }
    }

    [Table("rcpingunt")]
    [Serializable]
    public class RecipeIngredientUnit
    {
        [Key]
        [Column("nam")]
        public string Name { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
