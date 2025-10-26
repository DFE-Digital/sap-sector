namespace SAPSec.Core.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Models;
using System.Text.Json;

public class OrganisationService : IOrganisationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OrganisationService> _logger;

    public OrganisationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrganisationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public OrganisationDetails? GetUserOrganisation()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Get organisation claim from user
        var orgClaim = user.FindFirst("organisation")?.Value;
        if (string.IsNullOrEmpty(orgClaim) || orgClaim == "{}")
        {
            _logger.LogWarning("User {UserId} has no organisation",
                user.FindFirst("sub")?.Value);
            return null;
        }

        try
        {
            var org = JsonSerializer.Deserialize<OrganisationDetails>(orgClaim,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _logger.LogInformation(
                "Retrieved organisation: {OrgName} (URN: {Urn})",
                org?.Name,
                org?.Urn);

            return org;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse organisation claim");
            return null;
        }
    }
}