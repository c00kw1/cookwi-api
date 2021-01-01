using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Api.Service.Models
{
    public abstract class MongoEntity : IMongoEntity
    {
        [BsonId]
        [BsonRequired]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement("_ownerId")]
        [BsonRepresentation(BsonType.String)]
        [BsonDefaultValue("")]
        public string OwnerId { get; set; }

        [BsonRequired]
        [BsonElement("_creationDate")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreationDate { get; set; }

        [BsonRequired]
        [BsonElement("_updateDate")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdateDate { get; set; }
    }
}
