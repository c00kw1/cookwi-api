using Api.Service.Models;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Api.Hosting.Dto
{
    public class ContactDto : DtoObject
    {
        [JsonProperty("email")]
        [SwaggerSchema("Email address")]
        public string Email { get; set; }

        [JsonProperty("type")]
        [SwaggerSchema("Type of message")]
        [EnumDataType(typeof(ContactType))]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContactType Type { get; set; }

        [JsonProperty("message")]
        [SwaggerSchema("Message")]
        public string Message { get; set; }

        [JsonProperty("token")]
        [SwaggerSchema("ReCaptcha token")]
        public string Token { get; set; }

    }

    public static class ContactDtoExtensions
    {
        public static ContactDto Dto(this Contact entity)
        {
            return new ContactDto
            {
                Email = entity.Email,
                Type = entity.Type,
                Message = entity.Message
            };
        }

        public static Contact Model(this ContactDto contact)
        {
            return new Contact
            {
                Email = contact.Email,
                Type = contact.Type,
                Message = contact.Message
            };
        }
    }

    public class ContactDtoValidator : AbstractValidator<ContactDto>
    {
        public ContactDtoValidator()
        {
            RuleFor(c => c.Token).NotNull().NotEmpty().WithMessage("A reCaptcha token must be provided");
            RuleFor(c => c.Email).EmailAddress();
            RuleFor(c => c.Type).IsInEnum();
            RuleFor(c => c.Message).NotNull().NotEmpty().When(c => c.Type != ContactType.Access).WithMessage("A message must be provided");
        }
    }
}
