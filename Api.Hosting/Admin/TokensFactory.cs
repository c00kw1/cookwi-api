using Api.Hosting.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Api.Hosting.AdminAPI
{
    public class TokensFactory
    {
        private SsoSettings _settings;
        private AuthResponse _token;
        private DateTime _endTokenTime = DateTime.MinValue;

        private object _lockToken = new object();
        private object _lockTime = new object();

        public TokensFactory(IOptions<SsoSettings> ssoSettings)
        {
            _settings = ssoSettings?.Value;
        }

        public string AccessToken
        {
            get
            {
                lock (_lockToken)
                {
                    CheckToken();
                    return _token?.AccessToken;
                }
            }
        }

        private void CheckToken()
        {
            var needToGetToken = false;
            var needToRefreshToken = false;
            lock (_lockTime)
            {
                if (_endTokenTime == DateTime.MinValue)
                {
                    needToGetToken = true;
                }
                else
                {
                    needToRefreshToken = DateTime.Now > _endTokenTime;
                }
            }

            if (needToGetToken)
            {
                lock (_lockToken)
                {
                    _token = GetToken();
                }
            }
            else if (needToRefreshToken)
            {
                lock (_lockToken)
                {
                    _token = GetToken(true);
                }
            }
        }

        private AuthResponse GetToken(bool isRefresh = false)
        {
            try
            {
                var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", _settings.Api.ClientId),
                    new KeyValuePair<string, string>("client_secret", _settings.Api.ClientSecret)
                };

                if (!isRefresh)
                {
                    content.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                }
                else
                {
                    content.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                    content.Add(new KeyValuePair<string, string>("refresh_token", _token.RefreshToken));
                }


                using (var client = new HttpClient())
                {
                    var toSend = new FormUrlEncodedContent(content);
                    client.BaseAddress = new Uri(_settings.Api.BaseUrl);
                    var baseRes = client.PostAsync(_settings.Api.TokenUrl, toSend).GetAwaiter().GetResult();

                    if (baseRes.IsSuccessStatusCode)
                    {
                        var res = JsonConvert.DeserializeObject<AuthResponse>(baseRes.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                        lock (_lockTime)
                        {
                            _endTokenTime = DateTime.Now.AddSeconds(res.ExpiresIn - 30);
                        }

                        return res;
                    }
                    else if (isRefresh && (baseRes.StatusCode == HttpStatusCode.Forbidden ||
                                           baseRes.StatusCode == HttpStatusCode.Unauthorized ||
                                           baseRes.StatusCode == HttpStatusCode.BadRequest))
                    {
                        // last chance
                        // let's retry a proper get token, not a refresh
                        return GetToken(false);
                    }
                    else
                    {
                        throw new Exception($"Unable to get an access token ... Got {baseRes.StatusCode} response from API [{baseRes.RequestMessage.RequestUri}]");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public class AuthResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}