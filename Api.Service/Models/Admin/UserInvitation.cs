using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Api.Service.Models.Admin
{
    public class UserInvitation : MongoEntity
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        [BsonElement("expiration")]
        public DateTime Expiration { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]
        [BsonElement("used")]
        public bool Used { get; set; }

        public static UserInvitation GenerateNew()
        {
            return new UserInvitation
            {
                Used = false,
                Expiration = DateTime.UtcNow.AddDays(3)
            };
        }
    }
}
