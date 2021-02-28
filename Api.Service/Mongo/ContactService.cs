using Api.Service.Models;
using Api.Service.Settings;

namespace Api.Service.Mongo
{
    public class ContactService : MongoService<Contact>
    {
        public override string CollectionName => "contact";

        public ContactService(MongoDBSettings settings) : base(settings) { }
    }
}
