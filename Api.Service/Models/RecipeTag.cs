using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models
{
    [Table("rcptag")]
    public class RecipeTag
    {
        [Column("rcpid")]
        public Guid RecipeId { get; set; }

        [Column("tagid")]
        public Guid TagId { get; set; }

        public Recipe Recipe { get; set; }
        public Tag Tag { get; set; }
    }
}
