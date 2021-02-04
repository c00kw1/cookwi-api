using Api.Service.Settings;
using System.Collections.Generic;

namespace Api.Hosting.Settings
{
    public class SsoSettings : FileSettings<SsoSettings>
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string[] Issuers { get; set; }
        public Dictionary<string, string> Routes { get; set; }
        public ApiSettings Api { get; set; }
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; }
        public string TokenUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RedirectUrl { get; set; }
    }
}
