using SAPSec.Core.Authentication;
using System.Security.Claims;

namespace SAPSec.Test.Common.Authentication;

public class MockDsiUserService : IUserService
{
    private string _currentOrganisationId = "org-123";

    private readonly User _defaultUser;

    // The test sign-in handler adds its own email claim, so a configured test user email has to win over claims
    // for tests that switch between DfE and non-DfE users.
    private readonly string? _configuredEmail;

    public MockDsiUserService(TestUserOrganisation? testUserOrganisation = null)
    {
        _configuredEmail = testUserOrganisation?.Email;
        _defaultUser = CreateDefaultUser(
            testUserOrganisation?.CategoryId ?? TestUserOrganisation.DefaultCategoryId,
            testUserOrganisation?.Urn ?? TestUserOrganisation.DefaultUrn,
            testUserOrganisation?.Phase ?? TestUserOrganisation.DefaultPhase,
            testUserOrganisation?.Name ?? TestUserOrganisation.DefaultName,
            testUserOrganisation?.Email ?? TestUserOrganisation.NonDfeEmail);
    }

    private static User CreateDefaultUser(string categoryId, string urn, string phase, string organisationName, string email) => new()
    {
        Sub = "test-user-123",
        Email = email,
        GivenName = "Test",
        FamilyName = "User",
        Name = "Test User",
        Organisations = new List<Organisation>
        {
            new()
            {
                Id = "org-123",
                Name = organisationName,
                LegalName = "TEST ORGANISATION LTD",
                Urn = urn,
                Ukprn = "10012345",
                Category = new Category
                {
                    Id = categoryId,
                    Name = categoryId == OrganisationCategoryIds.Establishment ? "Establishment" : "Other"
                },
                Type = new Core.Authentication.Type { Id = "34", Name = "Academy Converter" },
                Status = new Status { Id = 1, Name = "Open", TagColor = "green" },
                Address = "123 Test Street, Test Town, TS1 1TT",
                PhaseOfEducation = new PhaseOfEducation { Id = 2, Name = phase }
            }
        }
    };

    public Task<User?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<User?>(null);
        }

        return Task.FromResult<User?>(_defaultUser);
    }

    public Task<Organisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<Organisation?>(null);
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

        return _configuredEmail
               ?? principal.FindFirst(ClaimTypes.Email)?.Value
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
