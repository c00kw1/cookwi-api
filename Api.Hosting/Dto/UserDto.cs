using Keycloak.Net.Models.Users;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Api.Hosting.Dto
{
    public class UserDto
    {
        [JsonProperty("email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [JsonProperty("password")]
        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage ="Passwords must be between 8 and 100 caracters")]
        public string Password { get; set; }

        [JsonProperty("firstName")]
        [Required]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        [Required]
        public string LastName { get; set; }
    }

    public static class UserDtoExtensions
    {
        public static UserDto ToDto(this User ssoUser)
        {
            return new UserDto
            {
                Email = ssoUser.Email,
                FirstName = ssoUser.FirstName,
                LastName = ssoUser.LastName,
                Password = ""
            };
        }
    }
}
