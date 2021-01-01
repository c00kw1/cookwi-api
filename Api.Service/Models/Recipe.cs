using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Api.Service.Models
{
    public class Recipe : MongoEntity
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("description")]
        public string Description { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("imagePath")]
        public string ImagePath { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("cookingTime")]
        public TimeSpan CookingTime { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("bakingTime")]
        public TimeSpan BakingTime { get; set; }

        [BsonElement("tags")]
        public HashSet<string> Tags { get; set; }

        [BsonElement("steps")]
        public List<RecipeStepMongo> Steps { get; set; }

        [BsonElement("ingredients")]
        public List<RecipeIngredientMongo> Ingredients { get; set; }
    }

    public class RecipeStepMongo
    {
        [BsonRepresentation(BsonType.Int32)]
        [BsonElement("position")]
        public int Position { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("content")]
        public string Content { get; set; }
    }

    public class RecipeIngredientMongo
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("quantity")]
        public double Quantity { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("unit")]
        public string Unit { get; set; }
    }
}
