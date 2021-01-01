using Api.Service.Models.Admin;
using Api.Service.Settings;

namespace Api.Service.Mongo
{
    public class UserInvitationsService : MongoService<UserInvitation>
    {
        public override string CollectionName => "user-invitations";

        public UserInvitationsService(MongoDBSettings settings) : base(settings) { }
    }
}
