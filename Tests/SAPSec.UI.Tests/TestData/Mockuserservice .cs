using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Security.Claims;

namespace SAPSec.UI.Tests.Mocks;

/// <summary>
/// Mock implementation of IUserService for UI testing.
/// Returns a test Establishment user by default.
/// </summary>
public class MockUserService : IUserService
{
    #region Test Data

    private static class TestData
    {
        public const string UserId = "test-user-id";
        public const string UserName = "Test User";
        public const string UserEmail = "test.user@school.gov.uk";

        public const string OrganisationId = "test-org-id";
        public const string OrganisationName = "Test Academy";
        public const string EstablishmentCategory = "Establishment";

        public const int CategoryId = 1;
    }

    #endregion

    #region Test User & Organisation

    private static Organisation CreateTestOrganisation()
    {
        return new Organisation
        {
            Id = TestData.OrganisationId,
            Name = TestData.OrganisationName,
            Category = new Category
            {
                Id = TestData.CategoryId.ToString(),
                Name = TestData.EstablishmentCategory
            }
        };
    }

    private static User CreateTestUser()
    {
        var organisation = CreateTestOrganisation();

        return new User
        {
            Name = TestData.UserName,
            Email = TestData.UserEmail,
            Organisations = new List<Organisation> { organisation }
        };
    }

    #endregion

    #region IUserService Implementation

    public Task<User?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        return Task.FromResult<User?>(CreateTestUser());
    }

    public Task<Organisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
    {
        return Task.FromResult<Organisation?>(CreateTestOrganisation());
    }

    public Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId)
    {
        return Task.FromResult(true);
    }

    public bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return principal?.Identity?.IsAuthenticated ?? false;
    }

    public string? GetUserId(ClaimsPrincipal principal)
    {
        return TestData.UserId;
    }

    public string? GetUserEmail(ClaimsPrincipal principal)
    {
        return TestData.UserEmail;
    }

    public string? GetUserName(ClaimsPrincipal principal)
    {
        return TestData.UserName;
    }

    public bool HasRole(ClaimsPrincipal principal, string role)
    {
        return true;
    }

    #endregion
}