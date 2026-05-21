using SAPSec.Core.Model;
using SAPSec.Core.Services.Helper;

namespace SAPSec.Core.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public void DeserializeToList_WhenOrganisationClaimContainsNumericPimsStatus_DeserializesOrganisation()
    {
        var organisationClaimJson = """
            [
              {
                "id": "org-1",
                "name": "Claim organisation",
                "pimsStatus": 1
              }
            ]
            """;

        var result = organisationClaimJson.DeserializeToList<Organisation>();

        result.Should().ContainSingle();
        result[0].Id.Should().Be("org-1");
        result[0].Name.Should().Be("Claim organisation");
        result[0].PimsStatus.Should().Be(1);
    }

    [Fact]
    public void DeserializeToList_WhenOrganisationClaimContainsStringPimsStatus_DeserializesOrganisation()
    {
        var organisationClaimJson = """
            [
              {
                "id": "org-1",
                "name": "Claim organisation",
                "pimsStatus": "1"
              }
            ]
            """;

        var result = organisationClaimJson.DeserializeToList<Organisation>();

        result.Should().ContainSingle();
        result[0].Id.Should().Be("org-1");
        result[0].Name.Should().Be("Claim organisation");
        result[0].PimsStatus.Should().Be(1);
    }
}
