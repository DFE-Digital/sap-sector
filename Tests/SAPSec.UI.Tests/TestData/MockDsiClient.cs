using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.UI.Tests.Mocks;

/// <summary>
/// Mock implementation of IDsiClient for UI testing.
/// Returns test data without making actual API calls.
/// </summary>
public class MockDsiClient : IDsiClient
{
    #region Test Data

    private static class TestData
    {
        public const string UserId = "test-user-id";
        public const string UserName = "Test User";
        public const string UserEmail = "test.user@school.gov.uk";

        public const string OrganisationId = "test-org-id";
        public const string OrganisationName = "Test Academy";

        public const string BearerToken = "test-bearer-token-for-ui-tests";
    }

    #endregion

    #region IDsiClient Implementation

    public Task<UserInfo?> GetUserAsync(string userId)
    {
        return Task.FromResult<UserInfo?>(CreateUserInfo(userId, TestData.UserEmail));
    }

    public Task<UserInfo?> GetUserByIdAsync(string userId)
    {
        return Task.FromResult<UserInfo?>(CreateUserInfo(userId, TestData.UserEmail));
    }

    public Task<UserInfo?> GetUserByEmailAsync(string email)
    {
        return Task.FromResult<UserInfo?>(CreateUserInfo(TestData.UserId, email));
    }

    public Task<Organisation?> GetOrganisationAsync(string organisationId)
    {
        return Task.FromResult<Organisation?>(CreateOrganisation(organisationId));
    }

    public Task<Organisation?> GetOrganisationByIdAsync(string organisationId)
    {
        return Task.FromResult<Organisation?>(CreateOrganisation(organisationId));
    }

    public Task<string> GenerateBearerToken()
    {
        return Task.FromResult(TestData.BearerToken);
    }

    public Task<string> GenerateBearerTokenAsync()
    {
        return Task.FromResult(TestData.BearerToken);
    }

    #endregion

    #region Factory Methods

    private static UserInfo CreateUserInfo(string userId, string email)
    {
        return new UserInfo
        {
            Sub = userId,
            Email = email,
            GivenName = "Test",
            FamilyName = "User"
        };
    }

    private static Organisation CreateOrganisation(string organisationId)
    {
        return new Organisation
        {
            Id = organisationId,
            Name = TestData.OrganisationName,
            Category = new Category
            {
                Id = "1",
                Name = "Establishment"
            }
        };
    }

    #endregion
}