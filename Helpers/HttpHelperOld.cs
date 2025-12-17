using Microsoft.Extensions.Configuration;
using OctopusData.Models;
using OctopusData.Models.Account;
using OctopusData.Models.Usage;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OctopusData.Helpers;

public class HttpHelperOld
{
    private readonly IConfigurationRoot _configuration;
    private Logger? _logger;

    private string _accountId;
    private string _apiKey;

    public HttpHelperOld(IConfigurationRoot configuration, string accountId, string apiKey)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _configuration = configuration;
        _accountId = accountId;
        _apiKey = apiKey;
    }

    public void SetLogger(Logger logger)
    {
        _logger = logger;
    }

    public bool Login(out Details? details)
    {
        var result = false;
        details = null;

        try
        {
            using (var httpClient = new HttpClient())
            {
                // "LoginUri": "https://api.octopus.energy/v1/accounts/{0}/",
                var uri = ConfigHelper.GetString(_configuration, "LoginUri", string.Empty);
                var requestUri = string.Format(uri, _accountId);

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                LogRequest(request);

                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    details = JsonSerializer.Deserialize<Details>(responseContent);
                    result = true;
                }
                else
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    Debugger.Break();
                }
            }

        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
        }

        return result;
    }

    public async Task<Usage?> ObtainElectricHalfHourlyUsage(OctopusAccount account, DateTime date)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                // "ElectricHalfHourlyUri": "https://api.octopus.energy/v1/electricity-meter-points/{0}/meters/{1}/consumption?period_from={2}T00:00:00Z&period_to={2}T23:59:59Z",
                var uri = ConfigHelper.GetString(_configuration, "ElectricHalfHourlyUri", string.Empty);
                var requestUri = string.Format(uri, account.ElectricMpan, account.ElectricMeterSerial, date.ToString("yyyy-MM-dd"));

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                LogRequest(request);

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var usage = JsonSerializer.Deserialize<Usage>(responseContent);
                    return usage;
                }
                else
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    Debugger.Break();
                }
            }

        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
        }

        return null;
    }

    public async Task<Usage?> ObtainGasHalfHourlyUsage(OctopusAccount account, DateTime date)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                // "ElectricHalfHourlyUri": "https://api.octopus.energy/v1/electricity-meter-points/{0}/meters/{1}/consumption?period_from={2}T00:00:00Z&period_to={2}T23:59:59Z",
                var uri = ConfigHelper.GetString(_configuration, "GasHalfHourlyUri", string.Empty);
                var requestUri = string.Format(uri, account.GasMprn, account.GasMeterSerial, date.ToString("yyyy-MM-dd"));

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredentials());
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                LogRequest(request);

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var usage = JsonSerializer.Deserialize<Usage>(responseContent);
                    return usage;
                }
                else
                {
                    _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                    Debugger.Break();
                }
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
        }
        return null;
    }

    private string EncodeCredentials()
    {
        // Encode credentials
        var byteArray = Encoding.ASCII.GetBytes($"{_apiKey}:");
        return Convert.ToBase64String(byteArray);
    }

    private void LogRequest(HttpRequestMessage request)
    {
        Debug.WriteLine("=== Request Diagnostic ===");
        Debug.WriteLine($"URI: {request.RequestUri}");
        Debug.WriteLine($"Method: {request.Method}");

        foreach (var header in request.Headers)
        {
            Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content?.Headers != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }
        Debug.WriteLine("==========================");
    }

}