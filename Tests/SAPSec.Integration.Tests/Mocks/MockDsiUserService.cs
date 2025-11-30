using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Security.Claims;

namespace SAPSec.Integration.Tests.Mocks;

public class MockDsiUserService : IDsiUserService
{
    private string _currentOrganisationId = "org-123";

    private readonly DsiUser _defaultUser = new()
    {
        Sub = "test-user-123",
        Email = "test@example.com",
        GivenName = "Test",
        FamilyName = "User",
        Name = "Test User",
        Organisations = new List<DsiOrganisation>
        {
            new()
            {
                Id = "org-123",
                Name = "Test Organisation",
                LegalName = "TEST ORGANISATION LTD",
                Urn = "123456",
                Ukprn = "10012345",
                Category = new DsiCategory { Id = "001", Name = "Establishment" },
                Type = new DsiType { Id = "34", Name = "Academy Converter" },
                Status = new DsiStatus { Id = 1, Name = "Open", TagColor = "green" },
                Address = "123 Test Street, Test Town, TS1 1TT",
                PhaseOfEducation = new DsiPhaseOfEducation { Id = 2, Name = "Primary" }
            }
        }
    };

    public Task<DsiUser?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<DsiUser?>(null);
        }

        return Task.FromResult<DsiUser?>(_defaultUser);
    }

    public Task<DsiOrganisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<DsiOrganisation?>(null);
        }

        var org = _defaultUser.Organisations.FirstOrDefault(o => o.Id == _currentOrganisationId)
                  ?? _defaultUser.Organisations.FirstOrDefault();

        return Task.FromResult(org);
    }

    public Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(false);
        }

        var org = _defaultUser.Organisations.FirstOrDefault(o => o.Id == organisationId);
        if (org == null)
        {
            return Task.FromResult(false);
        }

        _currentOrganisationId = organisationId;
        return Task.FromResult(true);
    }

    public string? GetUserId(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? principal.FindFirst("sub")?.Value
               ?? _defaultUser.Sub;
    }

    public string? GetUserEmail(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.Email)?.Value
               ?? principal.FindFirst("email")?.Value
               ?? _defaultUser.Email;
    }

    public bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return principal?.Identity?.IsAuthenticated == true;
    }

    public bool HasRole(ClaimsPrincipal principal, string role)
    {
        return principal?.IsInRole(role) == true;
    }
}