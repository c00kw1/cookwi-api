using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Service.Models.Admin
{
    [Table("usrinv")]
    [Serializable]
    public class UserInvitation
    {
        public static UserInvitation GenerateNew(double validForHours = 48)
        {
            return new UserInvitation
            {
                DateCreation = DateTime.Now,
                DateExpiration = DateTime.Now.AddHours(validForHours),
                Used = false
            };
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("datadd")]
        public DateTime DateCreation { get; set; }

        [Column("datexp")]
        public DateTime DateExpiration { get; set; }

        [Column("use")]
        public bool Used { get; set; }
    }
}
