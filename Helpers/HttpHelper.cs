using Microsoft.Extensions.Configuration;
using OctopusData.Models;
using OctopusData.Models.Account;
using OctopusData.Models.Charging.Devices;
using OctopusData.Models.Charging.Sessions;
using OctopusData.Models.Usage;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OctopusData.Models.ElectricCost;
using OctopusData.Models.GasCost;

namespace OctopusData.Helpers
{
    public class HttpHelper
    {
        private readonly IConfigurationRoot _configuration;
        private Logger? _logger;

        private readonly string _accountId;
        private readonly string _apiKey;

        private bool _saveResponses;

        private string? _krakenToken;

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
            _saveResponses = ConfigHelper.GetBoolean(_configuration, "SaveResponses", false);

            var uri = ConfigHelper.GetString(_configuration, "LoginUri", string.Empty);
            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri, _accountId);

                return await GetWithRedirect<Details>(requestUri, "Login");
            }

            return null;
        }

        #region Usage With Costs

        public async Task<GasCosts?> ObtainGasHalfHourlyCostsAsync(OctopusAccount account, DateTime requestedDate)
        {
            string requestUri = "https://api.octopus.energy/v1/graphql/";

            var query = string.Join("\\n", ResourceHelper.GetStringResource("GraphQL.GasCosts.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.GasCosts.json");

            graphQl = graphQl
                .Replace("[[Account-Number]]", account.Id)
                .Replace("[[StartOfThisPeriod]]", DateHelper.StartOfToday(requestedDate))
                .Replace("[[StartOfNextPeriod]]", DateHelper.StartOfTomorrow(requestedDate))
                .Replace("[[Gas-Supply-Point]]", account.GasMprn)
                .Replace("[[query]]", query);

            GasCosts? costs = await PostWithRedirect<GasCosts>(requestUri, graphQl, $"GraphQL.GasCosts-{DateHelper.IsoDateOnly(requestedDate)}");

            return costs;
        }

        public async Task<GasCosts?> ObtainElectricHalfHourlyCostsAsync(OctopusAccount account, DateTime requestedDate)
        {
            string requestUri = "https://api.octopus.energy/v1/graphql/";

            var query = string.Join("\\n", ResourceHelper.GetStringResource("GraphQL.ElectricCostsV1.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.ElectricCostsV1.json");

            graphQl = graphQl
                .Replace("[[Account-Number]]", account.Id)
                .Replace("[[StartOfThisPeriod]]", DateHelper.StartOfToday(requestedDate))
                .Replace("[[StartOfNextPeriod]]", DateHelper.StartOfTomorrow(requestedDate))
                .Replace("[[Electric-Supply-Point]]", account.ElectricMpan)
                .Replace("[[query]]", query);

            GasCosts? costs = await PostWithRedirect<GasCosts>(requestUri, graphQl, $"GraphQL.ElectricCostsV1-{DateHelper.IsoDateOnly(requestedDate)}");

            return costs;
        }

        public async Task<ElectricCosts?> ObtainElectricUsageCostsAsync(OctopusAccount account, DateTime requestedDate)
        {
            string requestUri = "https://api.octopus.energy/v1/graphql/";

            var query = string.Join("\\n", ResourceHelper.GetStringResource("GraphQL.ElectricCostsV2.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.ElectricCostsV2.json");

            graphQl = graphQl
                .Replace("[[Account-Number]]", account.Id)
                .Replace("[[StartOfThisPeriod]]", DateHelper.StartOfToday(requestedDate))
                .Replace("[[StartOfNextPeriod]]", DateHelper.StartOfTomorrow(requestedDate))
                .Replace("[[Electric-Supply-Point]]", account.ElectricMpan)
                .Replace("[[query]]", query);

            ElectricCosts? costs = await PostWithRedirect<ElectricCosts>(requestUri, graphQl, $"GraphQL.ElectricCostsV2-{DateHelper.IsoDateOnly(requestedDate)}");

            return costs;
        }

        #endregion With Costs

        #region Charging Sessions

        public async Task<Chargers?> ObtainChargersAsync(OctopusAccount account, DateTime requestedDate, DateTime goLive)
        {
            string requestUri = "https://api.octopus.energy/v1/graphql/";

            var query = string.Join("\\n", ResourceHelper.GetStringResource("GraphQL.Chargers.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.Chargers.json");
            graphQl = graphQl
                .Replace("[[Account-Number]]", account.Id)
                .Replace("[[StartOfMonth]]", DateHelper.FirstDayOfThisMonth(requestedDate))
                .Replace("[[GoLive-Date]]", DateHelper.IsoDateTime(goLive))
                .Replace("[[query]]", query);

            Chargers? devices = await PostWithRedirect<Chargers>(requestUri, graphQl, $"GraphQL.Chargers-{DateHelper.IsoDateOnly(requestedDate)}");

            return devices;
        }

        public async Task<ChargeHistory?> ObtainChargeHistoryAsync(OctopusAccount account, DateTime requestedDate, string chargerId)
        {
            string requestUri = "https://api.octopus.energy/v1/graphql/";

            var query = string.Join("\\n", ResourceHelper.GetStringResource("GraphQL.ChargeHistory.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.ChargeHistory.json");
            graphQl = graphQl
                .Replace("[[Account-Number]]", account.Id)
                .Replace("[[StartOfMonth]]", DateHelper.FirstDayOfThisMonth(requestedDate))
                .Replace("[[StartOfNextMonth]]", DateHelper.FirstDayOfNextMonth(requestedDate, true))
                .Replace("[[Device-Id]]", chargerId)
                .Replace("[[query]]", query);

            ChargeHistory? history = await PostWithRedirect<ChargeHistory>(requestUri, graphQl, $"GraphQL.ChargeHistory-{DateHelper.IsoDateOnly(requestedDate)}");

            return history;
        }

        #endregion Charging

        #region HalfHourly Usage

        public async Task<Usage?> ObtainElectricHalfHourlyUsageAsync(OctopusAccount account, DateTime requestedDate)
        {
            var uri = ConfigHelper.GetString(_configuration, "ElectricHalfHourlyUri", string.Empty);

            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri,
                    account.ElectricMpan,
                    account.ElectricMeterSerial,
                    requestedDate.ToString("yyyy-MM-dd"));

                return await GetWithRedirect<Usage>(requestUri, $"ElectricHalfHourly-{DateHelper.IsoDateOnly(requestedDate)}");
            }

            return null;
        }

        public async Task<Usage?> ObtainGasHalfHourlyUsageAsync(OctopusAccount account, DateTime requestedDate)
        {
            var uri = ConfigHelper.GetString(_configuration, "GasHalfHourlyUri", string.Empty);

            if (!string.IsNullOrEmpty(uri))
            {
                var requestUri = string.Format(uri,
                    account.GasMprn,
                    account.GasMeterSerial,
                    requestedDate.ToString("yyyy-MM-dd"));

                return await GetWithRedirect<Usage>(requestUri, $"GasHalfHourly-{DateHelper.IsoDateOnly(requestedDate)}");
            }
            return null;
        }

        #endregion HalfHourly Usage

        private async Task<T?> GetWithRedirect<T>(string requestUri, string requestType)
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

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    _logger?.WriteLine(responseContent);
                    return default;
                }

                if (_saveResponses)
                {
                    _logger?.DumpJson(requestType, responseContent);
                }

                return JsonSerializer.Deserialize<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger?.WriteLine(ex.ToString());
                return default;
            }
        }

        private async Task<T?> PostWithRedirect<T>(string requestUri, string body, string requestType)
        {
            try
            {
                if (string.IsNullOrEmpty(_krakenToken) && !body.Contains("ObtainJSONWebTokenInput"))
                {
                    _krakenToken = await FetchKrakenToken(requestUri);
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                if (!string.IsNullOrEmpty(_krakenToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {_krakenToken}");
                }
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(body, null, "application/json");
                request.Content = content;

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

                        using var followUp = new HttpRequestMessage(HttpMethod.Post, redirectUri);
                        followUp.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                        followUp.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        response = await Client.SendAsync(followUp);
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    _logger?.WriteLine(responseContent);
                    return default;
                }

                if (_saveResponses)
                {
                    _logger?.DumpJson(requestType, responseContent);
                }

                return JsonSerializer.Deserialize<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger?.WriteLine(ex.ToString());
                return default;
            }
        }

        private async Task<string?> FetchKrakenToken(string requestUri)
        {
            var graphQl = ResourceHelper.GetStringResource("GraphQL.ObtainKrakenToken.json");
            graphQl = graphQl.Replace("[[API-Key]]", _apiKey);

            KrakenResponse? response = await PostWithRedirect<KrakenResponse>(requestUri, graphQl, "ObtainKrakenToken");

            string? token = response?.Data.ObtainKrakenToken.Token;

            return token;
        }

        private string EncodeCredentials()
        {
            var byteArray = Encoding.ASCII.GetBytes($"{_apiKey}:");
            return Convert.ToBase64String(byteArray);
        }
    }
}