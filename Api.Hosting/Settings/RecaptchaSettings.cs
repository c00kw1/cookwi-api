using Api.Service.Settings;

namespace Api.Hosting.Settings
{
    public class RecaptchaSettings : FileSettings<RecaptchaSettings>
    {
        public string ClientKey { get; set; }
        public string ServerKey { get; set; }
    }
}
