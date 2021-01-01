using Api.Service.Models;
using Api.Service.Settings;
using System.Linq;

namespace Api.Service.Mongo
{
    public class RecipesService : MongoService<Recipe>
    {
        public override string CollectionName => "recipes";

        public RecipesService(MongoDBSettings settings) : base(settings) { }

        public string[] GetAllTags(string userId)
        {
            return GetAll(userId).SelectMany(r => r.Tags).Distinct().ToArray();
        }
    }
}
