namespace Api.Hosting.Settings
{
    public class S3Settings : SettingsBase<S3Settings>
    {
        public string Endpoint { get; set; }
        public bool Ssl { get; set; }
        public string Bucket { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
    }
}
