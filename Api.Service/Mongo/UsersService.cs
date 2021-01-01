using Api.Service.Models;
using Api.Service.Settings;

namespace Api.Service.Mongo
{
    public class UsersService : MongoService<User>
    {
        public override string CollectionName => "users";

        public UsersService(MongoDBSettings settings) : base(settings) { }
    }
}
