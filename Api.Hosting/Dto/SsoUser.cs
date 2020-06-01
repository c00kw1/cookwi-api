using Newtonsoft.Json;

namespace Api.Hosting.Dto
{
    public class SsoUser
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("groups")]
        public string[] Groups { get; set; }

        public SsoUser(UserDto userDto)
        {
            Enabled = true;
            Email = userDto.Email;
            EmailVerified = true;
            FirstName = userDto.FirstName;
            LastName = userDto.LastName.ToUpper();
            Groups = new[] { "users" };
        }
    }
}