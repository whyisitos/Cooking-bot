using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CookingBot.Models
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public string Instructions { get; set; } = string.Empty;
        public int Calories { get; set; } = 0;
    }
}