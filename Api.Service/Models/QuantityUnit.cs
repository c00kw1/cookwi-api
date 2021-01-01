using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Service.Models
{
    public class QuantityUnit : MongoEntity
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("acronym")]
        public string Acronym { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("type")]
        public UnitType Type { get; set; }
    }

    public enum UnitType
    {
        Liquid, // L, cL, mL
        Weight, // kg, g
        Size, // cm
        Container, // spoon, glass
    }
}
