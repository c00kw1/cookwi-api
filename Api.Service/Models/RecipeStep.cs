using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models
{
    [Table("rcpstp")]
    [Serializable]
    public class RecipeStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("rcpid")]
        public Guid RecipeId { get; set; }

        [Column("stpnum")]
        public int StepNumber { get; set; }

        [Column("stptxt")]
        public string Content { get; set; }

        public Recipe Recipe { get; set; }
    }
}
