using Api.Service.Settings;
using System;
using System.Collections.Generic;

namespace Api.Hosting.Settings
{
    public class SsoSettings : FileSettings<SsoSettings>
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string[] Issuers { get; set; }
        public Dictionary<string, string> Routes { get; set; }
        public SsoApiSettings Api { get; set; }
    }

    public class SsoApiSettings
    {
        public string BaseUrl { get; set; }
        public string TokenUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string CredentialsPath { get; set; }

        public string ClientId => _credentials.Value.ClientId;
        public string ClientSecret => _credentials.Value.ClientSecret;

        [NonSerialized]
        private Lazy<SsoApiCredentials> _credentials;

        public SsoApiSettings()
        {
            _credentials = new Lazy<SsoApiCredentials>(() => { return SsoApiCredentials.Get(CredentialsPath); });
        }
    }

    public class SsoApiCredentials : FileSettings<SsoApiCredentials>
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
