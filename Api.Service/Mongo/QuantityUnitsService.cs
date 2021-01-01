using Api.Service.Models;
using Api.Service.Settings;

namespace Api.Service.Mongo
{
    public class QuantityUnitsService : MongoService<QuantityUnit>
    {
        public override string CollectionName => "quantity-units";

        public QuantityUnitsService(MongoDBSettings settings) : base(settings) { }
    }
}
