using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Services;

public class DsiApiService : IDsiClient
{
    private readonly HttpClient _httpClient;
    private readonly DsiConfiguration _config;
    private readonly ILogger<DsiApiService> _logger;

    public DsiApiService(
        HttpClient httpClient,
        IOptions<DsiConfiguration> config,
        ILogger<DsiApiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrEmpty(_config.ApiUri))
        {
            _logger.LogWarning("DsiConfiguration:ApiUri is not configured - API calls will fail");
        }
        else
        {
            try
            {
                _httpClient.BaseAddress = new Uri(_config.ApiUri);
                _logger.LogInformation("DSI API BaseAddress set to: {BaseAddress}", _config.ApiUri);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid ApiUri format: {ApiUri}", _config.ApiUri);
                throw;
            }
        }
    }

    public async Task<UserInfo?> GetUserAsync(string userId)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/users/{userId}/organisations");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to get user {UserId} from DSI API. Status: {StatusCode}. Body: {Body}",
                    userId,
                    response.StatusCode,
                    body);
                return null;
            }

            var organisations = await response.Content.ReadFromJsonAsync<List<Organisation>>();

            return new UserInfo
            {
                Sub = userId,
                Organisations = organisations ?? new List<Organisation>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId} from DSI API", userId);
            throw;
        }
    }

    public async Task<UserInfo?> GetUserByEmailAsync(string email)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/users/by-email/{email}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to get user by email {Email} from DSI API. Status: {StatusCode}. Body: {Body}",
                    email,
                    response.StatusCode,
                    body);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email} from DSI API", email);
            throw;
        }
    }

    public async Task<Organisation?> GetOrganisationAsync(string organisationId)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/organisations/{organisationId}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Failed to get organisation {OrganisationId} from DSI API. Status: {StatusCode}. Body: {Body}",
                    organisationId,
                    response.StatusCode,
                    body);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Organisation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organisation {OrganisationId} from DSI API", organisationId);
            throw;
        }
    }

    public Task<string> GenerateBearerToken()
    {
        try
        {
            var expires = DateTimeOffset.UtcNow.AddMinutes(_config.TokenExpiryMinutes);

            var header = new Dictionary<string, object>
            {
                ["alg"] = "HS256",
                ["typ"] = "JWT"
            };

            var payload = new Dictionary<string, object>
            {
                ["iss"] = _config.ClientId,
                ["aud"] = _config.Audience,
                ["exp"] = expires.ToUnixTimeSeconds()
            };

            var encodedHeader = Base64UrlEncoder.Encode(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)));
            var encodedPayload = Base64UrlEncoder.Encode(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
            var unsignedToken = $"{encodedHeader}.{encodedPayload}";

            using var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_config.ApiSecret));
            var signature = Base64UrlEncoder.Encode(
                hmac.ComputeHash(Encoding.ASCII.GetBytes(unsignedToken)));

            return Task.FromResult($"{unsignedToken}.{signature}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bearer token for DSI API");
            throw;
        }
    }
}
