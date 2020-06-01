using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models
{
    [Table("usr")]
    [Serializable]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("bthdat")]
        public DateTime BirthDate { get; set; }

        [Column("gen")]
        public string Gender { get; set; }
    }
}
