using Serilog.Sinks.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SAPSec.Web.Logging;

public class LogitHttpClient : IHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public LogitHttpClient(string apiKey)
    {
        _apiKey = apiKey;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        var content = new StreamContent(contentStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };

        request.Headers.Add("ApiKey", _apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "SAPSec-Serilog/1.0");

        return await _httpClient.SendAsync(request);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public void Configure(IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

}