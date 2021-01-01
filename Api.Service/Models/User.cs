using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Api.Service.Models
{
    public class User : MongoEntity
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [BsonElement("ssoId")]
        public Guid SsoId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        [BsonElement("birthdate")]
        public DateTime Birthdate { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [BsonElement("gender")]
        public Gender Gender { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}
