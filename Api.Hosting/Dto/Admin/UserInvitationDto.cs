using Api.Service.Models.Admin;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Api.Hosting.Dto.Admin
{
    public class UserInvitationDto
    {
        [JsonProperty("id")]
        [SwaggerSchema("The id to use for user registration", ReadOnly = true)]
        public string Id { get; set; }

        [JsonProperty("expiration")]
        [SwaggerSchema("The expiration date for this invitation", ReadOnly = true)]
        public DateTime Expiration { get; set; }

        [JsonProperty("used")]
        [SwaggerSchema("Has this invitation already been used ?", ReadOnly = true)]
        public bool Used { get; set; }
    }

    public static class UserInvitationDtoExtensions
    {
        public static UserInvitationDto Dto(this UserInvitation invitation)
        {
            return new UserInvitationDto
            {
                Id = invitation.Id,
                Expiration = invitation.Expiration,
                Used = invitation.Used
            };
        }
    }
}
