using Microsoft.Extensions.Configuration;
using OctopusData.Models;
using OctopusData.Models.Account;
using OctopusData.Models.Usage;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OctopusData.Helpers
{
    public class HttpHelper
    {
        private readonly IConfigurationRoot _configuration;
        private Logger? _logger;

        private readonly string _accountId;
        private readonly string _apiKey;

        // HttpClient without auto-redirect
        private static readonly HttpClient Client = new(
            new HttpClientHandler
            {
                AllowAutoRedirect = false, // we’ll handle redirects manually
                UseCookies = false
            });

        public HttpHelper(IConfigurationRoot configuration, string accountId, string apiKey)
        {
            _configuration = configuration;
            _accountId = accountId;
            _apiKey = apiKey;
        }

        public void SetLogger(Logger logger) => _logger = logger;

        public async Task<Details?> LoginAsync()
        {
            var uri = ConfigHelper.GetString(_configuration, "LoginUri", string.Empty);
            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri, _accountId);

                return await SendWithRedirect<Details>(requestUri);
            }

            return null;
        }

        public async Task<Usage?> ObtainElectricHalfHourlyUsageAsync(OctopusAccount account, DateTime currentDate)
        {
            var uri = ConfigHelper.GetString(_configuration, "ElectricHalfHourlyUri", string.Empty);

            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri,
                    account.ElectricMpan,
                    account.ElectricMeterSerial,
                    currentDate.ToString("yyyy-MM-dd"));

                return await SendWithRedirect<Usage>(requestUri);
            }

            return null;
        }

        public async Task<Usage?> ObtainGasHalfHourlyUsageAsync(OctopusAccount account, DateTime currentDate)
        {
            var uri = ConfigHelper.GetString(_configuration, "GasHalfHourlyUri", string.Empty);

            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri,
                    account.GasMprn,
                    account.GasMeterSerial,
                    currentDate.ToString("yyyy-MM-dd"));

                return await SendWithRedirect<Usage>(requestUri);
            }
            return null;
        }

        private async Task<T?> SendWithRedirect<T>(string requestUri)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.SendAsync(request);

                // Handle redirect manually
                if (response.StatusCode == HttpStatusCode.MovedPermanently
                    || response.StatusCode == HttpStatusCode.Redirect
                    || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    var redirectUri = response.Headers.Location;
                    if (redirectUri != null)
                    {
                        // If relative, combine with original request URI
                        if (!redirectUri.IsAbsoluteUri)
                        {
                            redirectUri = new Uri(new Uri(requestUri), redirectUri);
                        }

                        using var followUp = new HttpRequestMessage(HttpMethod.Get, redirectUri);
                        followUp.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                        followUp.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        response = await Client.SendAsync(followUp);
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine("Response body:");
                Console.WriteLine(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    _logger?.WriteLine(responseContent);
                    return default;
                }

                return JsonSerializer.Deserialize<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger?.WriteLine(ex.ToString());
                return default;
            }
        }

        private string EncodeCredentials()
        {
            var byteArray = Encoding.ASCII.GetBytes($"{_apiKey}:");
            return Convert.ToBase64String(byteArray);
        }
    }
}