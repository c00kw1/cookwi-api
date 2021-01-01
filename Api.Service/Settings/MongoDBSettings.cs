namespace Api.Service.Settings
{
    public class MongoDBSettings : FileSettings<MongoDBSettings>
    {
        public string Endpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }
}
