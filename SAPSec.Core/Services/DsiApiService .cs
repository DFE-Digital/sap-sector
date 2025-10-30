using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SAPSec.Core.Interfaces.Services.IDsiApiService;
using SAPSec.Core.Model.DsiConfiguration;
using SAPSec.Core.Model.DsiUser;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace SAPSec.Core.Services.DsiApiService;

public class DsiApiService : IDsiApiService
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
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri(_config.ApiUri);
    }

    public async Task<DsiUserInfo?> GetUserAsync(string userId)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get user {UserId} from DSI API. Status: {StatusCode}",
                    userId,
                    response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DsiUserInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId} from DSI API", userId);
            throw;
        }
    }

    public async Task<DsiUserInfo?> GetUserByEmailAsync(string email)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/users/by-email/{email}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get user by email {Email} from DSI API. Status: {StatusCode}",
                    email,
                    response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DsiUserInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email} from DSI API", email);
            throw;
        }
    }

    public async Task<DsiOrganisation?> GetOrganisationAsync(string organisationId)
    {
        try
        {
            var token = await GenerateBearerToken();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/organisations/{organisationId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get organisation {OrganisationId} from DSI API. Status: {StatusCode}",
                    organisationId,
                    response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DsiOrganisation>();
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.ApiSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim("iss", _config.ClientId),
                        new Claim("aud", _config.Audience)
                    }),
                Expires = DateTime.UtcNow.AddMinutes(_config.TokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bearer token for DSI API");
            throw;
        }
    }
}