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
        public Guid Id { get; set; }

        [JsonProperty("dateCreation")]
        [SwaggerSchema("The creation date of the invitation", ReadOnly = true)]
        public DateTime DateCreation { get; set; }

        [JsonProperty("dateExpiration")]
        [SwaggerSchema("The expiration date for this invitation", ReadOnly = true)]
        public DateTime DateExpiration { get; set; }

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
                DateCreation = invitation.DateCreation,
                DateExpiration = invitation.DateExpiration,
                Used = invitation.Used
            };
        }
    }
}
