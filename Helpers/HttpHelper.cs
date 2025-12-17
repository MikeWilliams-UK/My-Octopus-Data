using Microsoft.Extensions.Configuration;
using OctopusData.Models.Account;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OctopusData.Models;
using OctopusData.Models.Usage;

namespace OctopusData.Helpers;

public class HttpHelper
{
    private readonly IConfigurationRoot _configuration;
    private Logger? _logger;

    private readonly string _accountId;
    private readonly string _apiKey;

    // Client should be static/shared, but headers must be set per request
    //private static readonly HttpClient Client = new(
    //    new HttpClientHandler
    //    {
    //        UseCookies = false, // Octopus API doesn't rely on cookies
    //        AllowAutoRedirect = true
    //    });

    private static readonly HttpClient Client = new();

    public HttpHelper(IConfigurationRoot configuration, string accountId, string apiKey)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _configuration = configuration;
        _accountId = accountId;
        _apiKey = apiKey;
    }

    public void SetLogger(Logger logger) => _logger = logger;

    public async Task<Details?> LoginAsync()
    {
        try
        {
            var uri = ConfigHelper.GetString(_configuration, "LoginUri", string.Empty);
            var requestUri = string.Format(uri, _accountId);

            Debug.WriteLine(requestUri);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine("Auth header: " + request.Headers.Authorization);

            var response = await Client.SendAsync(request);

            // Log status and body for diagnostics
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine("Response body:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                _logger?.WriteLine(responseContent); // log error JSON
                return null;
            }

            return JsonSerializer.Deserialize<Details>(responseContent);
        }
        catch (Exception ex)
        {
            _logger?.WriteLine(ex.ToString());
            return null;
        }
    }

    public async Task<Usage?> ObtainElectricHalfHourlyUsageAsync(OctopusAccount account, DateTime currentDate)
    {
        try
        {
            var uri = ConfigHelper.GetString(_configuration, "ElectricHalfHourlyUri", string.Empty);
            var requestUri = string.Format(uri,
                account.ElectricMpan,
                account.ElectricMeterSerial,
                currentDate.ToString("yyyy-MM-dd"));

            Debug.WriteLine($"{account.Id} {account.ElectricMpan} {account.ElectricMeterSerial}");

            // Ensure full ISO timestamps in the template string
            requestUri = requestUri.Replace("T00:00Z", "T00:00:00Z").Replace("T23:59Z", "T23:59:59Z");

            Debug.WriteLine(requestUri);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine("Auth header: " + request.Headers.Authorization);

            var response = await Client.SendAsync(request);

            // Log status and body for diagnostics
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine("Response body:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                _logger?.WriteLine(responseContent); // log error JSON
                return null;
            }

            return JsonSerializer.Deserialize<Usage>(responseContent);
        }
        catch (Exception ex)
        {
            _logger?.WriteLine(ex.ToString());
            return null;
        }
    }

    public async Task<Usage?> ObtainGasHalfHourlyUsageAsync(OctopusAccount account, DateTime currentDate)
    {
        try
        {
            var uri = ConfigHelper.GetString(_configuration, "GasHalfHourlyUri", string.Empty);
            var requestUri = string.Format(uri,
                account.GasMprn,
                account.GasMeterSerial,
                currentDate.ToString("yyyy-MM-dd"));

            Debug.WriteLine($"{account.Id} {account.GasMprn} {account.GasMeterSerial}");

            // Ensure full ISO timestamps in the template string
            requestUri = requestUri.Replace("T00:00Z", "T00:00:00Z").Replace("T23:59Z", "T23:59:59Z");

            Debug.WriteLine(requestUri);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Debug.WriteLine("Auth header: " + request.Headers.Authorization);

            var response = await Client.SendAsync(request);

            // Log status and body for diagnostics
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine("Response body:");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                _logger?.WriteLine(responseContent); // log error JSON
                return null;
            }

            return JsonSerializer.Deserialize<Usage>(responseContent);
        }
        catch (Exception ex)
        {
            _logger?.WriteLine(ex.ToString());
            return null;
        }
    }

    private string EncodeCredentials()
    {
        var byteArray = Encoding.ASCII.GetBytes($"{_apiKey}:");
        return Convert.ToBase64String(byteArray);
    }
}