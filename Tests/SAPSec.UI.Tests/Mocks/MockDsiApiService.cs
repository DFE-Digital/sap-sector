using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.UI.Tests.Mocks;

public class MockDsiApiService : IDsiApiService
{
    private readonly Dictionary<string, DsiUserInfo> _users;
    private readonly Dictionary<string, DsiOrganisation> _organisations;

    public MockDsiApiService()
    {
        // Setup default test data
        _organisations = new Dictionary<string, DsiOrganisation>
        {
            ["org-123"] = new DsiOrganisation
            {
                Id = "org-123",
                Name = "Test Organisation",
                LegalName = "TEST ORGANISATION LTD",
                Urn = "123456",
                Ukprn = "10012345",
                EstablishmentNumber = "1234",
                Category = new DsiCategory { Id = "001", Name = "Establishment" },
                Type = new DsiType { Id = "34", Name = "Academy Converter" },
                Status = new DsiStatus { Id = 1, Name = "Open", TagColor = "green" },
                Address = "123 Test Street, Test Town, TS1 1TT",
                Telephone = "01onal234567890",
                Region = new DsiRegion { Id = "D", Name = "Yorkshire and the Humber" },
                LocalAuthority = new DsiLocalAuthority
                {
                    Id = "LA-123",
                    Name = "Test Council",
                    Code = "123"
                },
                PhaseOfEducation = new DsiPhaseOfEducation { Id = 2, Name = "Primary" },
                StatutoryLowAge = 3,
                StatutoryHighAge = 11
            },
            ["org-456"] = new DsiOrganisation
            {
                Id = "org-456",
                Name = "Secondary Test School",
                LegalName = "SECONDARY TEST SCHOOL",
                Urn = "654321",
                Ukprn = "10054321",
                Category = new DsiCategory { Id = "001", Name = "Establishment" },
                Type = new DsiType { Id = "34", Name = "Academy Converter" },
                Status = new DsiStatus { Id = 1, Name = "Open", TagColor = "green" },
                PhaseOfEducation = new DsiPhaseOfEducation { Id = 4, Name = "Secondary" },
                StatutoryLowAge = 11,
                StatutoryHighAge = 16
            }
        };

        _users = new Dictionary<string, DsiUserInfo>
        {
            ["test-user-123"] = new DsiUserInfo
            {
                Email = "test@example.com",
                GivenName = "Test",
                FamilyName = "User",
                Organisations = new List<DsiOrganisation> { _organisations["org-123"] }
            },
            ["test-user-456"] = new DsiUserInfo
            {
                Email = "another@example.com",
                GivenName = "Another",
                FamilyName = "User",
                Organisations = new List<DsiOrganisation>
                {
                    _organisations["org-123"],
                    _organisations["org-456"]
                }
            }
        };
    }

    public Task<DsiUserInfo?> GetUserAsync(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<DsiUserInfo?> GetUserByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<DsiOrganisation?> GetOrganisationAsync(string organisationId)
    {
        _organisations.TryGetValue(organisationId, out var org);
        return Task.FromResult(org);
    }

    public Task<string> GenerateBearerToken()
    {
        // Return a fake token for testing
        return Task.FromResult("test-bearer-token-12345");
    }
}
