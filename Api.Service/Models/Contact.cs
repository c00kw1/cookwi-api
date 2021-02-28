using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Service.Models
{
    public class Contact : MongoEntity
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("email")]
        public string Email { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("type")]
        public ContactType Type { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("message")]
        public string Message { get; set; }
    }

    public enum ContactType
    {
        Message,
        Bug,
        Access
    }
}
