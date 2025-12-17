using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace OctopusData.Helpers;

public class HttpHelper
{
    private readonly IConfigurationRoot _configuration;
    private Logger? _logger;

    private readonly HttpClient _httpClient1 = new();

    public HttpHelper(IConfigurationRoot configuration)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _configuration = configuration;
    }

    public void SetLogger(Logger logger)
    {
        _logger = logger;
    }

    public bool Login(string accountId, string apiKey)
    {
        var result = false;

        try
        {
            var loginUri = ConfigHelper.GetString(_configuration, "LoginUri", string.Empty);
            var requestUri = string.Format(loginUri, accountId);

            // Encode credentials
            var byteArray = Encoding.ASCII.GetBytes($"{apiKey}:");
            _httpClient1.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                Debug.WriteLine(responseContent);
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
        }

        return result;
    }
}