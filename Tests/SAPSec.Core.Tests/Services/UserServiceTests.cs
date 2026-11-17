using SAPSec.Core.Authentication;
using SAPSec.Core.Text;

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

    [Theory]
    [InlineData("001", true)]
    [InlineData("002", false)]
    [InlineData("010", false)]
    [InlineData("013", false)]
    public void DeserializeToList_WhenOrganisationClaimContainsCategory_SetsIsEstablishmentFromCategoryId(
        string categoryId, bool expectedIsEstablishment)
    {
        var organisationClaimJson = $$"""
            [
              {
                "id": "org-1",
                "name": "Claim organisation",
                "urn": "100052",
                "category": { "id": "{{categoryId}}", "name": "Any name" }
              }
            ]
            """;

        var result = organisationClaimJson.DeserializeToList<Organisation>();

        result.Should().ContainSingle();
        result[0].Category!.Id.Should().Be(categoryId);
        result[0].Urn.Should().Be("100052");
        result[0].IsEstablishment.Should().Be(expectedIsEstablishment);
    }

    [Fact]
    public void DeserializeToList_WhenOrganisationClaimHasNoCategory_IsNotAnEstablishment()
    {
        var organisationClaimJson = """
            [
              {
                "id": "org-1",
                "name": "Claim organisation"
              }
            ]
            """;

        var result = organisationClaimJson.DeserializeToList<Organisation>();

        result.Should().ContainSingle();
        result[0].Category.Should().BeNull();
        result[0].IsEstablishment.Should().BeFalse();
    }

    [Fact]
    public void DeserializeToList_WhenOrganisationClaimContainsAllFields_MapsEveryField()
    {
        var organisationClaimJson = """
            [
              {
                "id": "org-1",
                "name": "Hampstead School",
                "LegalName": "Hampstead School Legal",
                "category": { "id": "001", "name": "Establishment" },
                "type": { "id": "28", "name": "Academy Sponsor Led" },
                "urn": "100052",
                "uid": "uid-1",
                "upin": "upin-1",
                "ukprn": "10000001",
                "establishmentNumber": "4001",
                "status": { "id": 1, "name": "Open", "tagColor": "green" },
                "closedOn": "2030-01-02T00:00:00",
                "address": "1 School Road",
                "telephone": "0123456789",
                "region": { "id": "H", "name": "London" },
                "localAuthority": { "id": "la-1", "name": "Camden", "code": "202" },
                "phaseOfEducation": { "id": 4, "name": "Secondary" },
                "statutoryLowAge": 11,
                "statutoryHighAge": 18,
                "legacyId": "legacy-1",
                "companyRegistrationNumber": "12345678",
                "SourceSystem": "GIAS",
                "providerTypeName": "Academy",
                "ProviderTypeCode": 10,
                "GIASProviderType": "Academy converter",
                "PIMSProviderType": "Academy PIMS",
                "PIMSProviderTypeCode": 11,
                "PIMSStatusName": "Open PIMS",
                "pimsStatus": 1,
                "GIASStatusName": "Open GIAS",
                "GIASStatus": 2,
                "MasterProviderStatusName": "Open master",
                "MasterProviderStatusCode": 3,
                "OpenedOn": "2010-09-01T00:00:00",
                "DistrictAdministrativeName": "Camden district",
                "DistrictAdministrativeCode": "E09000007",
                "IsOnAPAR": "Yes"
              }
            ]
            """;

        var result = organisationClaimJson.DeserializeToList<Organisation>();

        result.Should().ContainSingle();
        var organisation = result[0];
        organisation.Id.Should().Be("org-1");
        organisation.Name.Should().Be("Hampstead School");
        organisation.LegalName.Should().Be("Hampstead School Legal");
        organisation.Category!.Id.Should().Be("001");
        organisation.Category.Name.Should().Be("Establishment");
        organisation.Type!.Id.Should().Be("28");
        organisation.Type.Name.Should().Be("Academy Sponsor Led");
        organisation.Urn.Should().Be("100052");
        organisation.Uid.Should().Be("uid-1");
        organisation.Upin.Should().Be("upin-1");
        organisation.Ukprn.Should().Be("10000001");
        organisation.EstablishmentNumber.Should().Be("4001");
        organisation.Status!.Id.Should().Be(1);
        organisation.Status.Name.Should().Be("Open");
        organisation.Status.TagColor.Should().Be("green");
        organisation.ClosedOn.Should().Be(new DateTime(2030, 1, 2));
        organisation.Address.Should().Be("1 School Road");
        organisation.Telephone.Should().Be("0123456789");
        organisation.Region!.Id.Should().Be("H");
        organisation.Region.Name.Should().Be("London");
        organisation.LocalAuthority!.Id.Should().Be("la-1");
        organisation.LocalAuthority.Name.Should().Be("Camden");
        organisation.LocalAuthority.Code.Should().Be("202");
        organisation.PhaseOfEducation!.Id.Should().Be(4);
        organisation.PhaseOfEducation.Name.Should().Be("Secondary");
        organisation.StatutoryLowAge.Should().Be(11);
        organisation.StatutoryHighAge.Should().Be(18);
        organisation.LegacyId.Should().Be("legacy-1");
        organisation.CompanyRegistrationNumber.Should().Be("12345678");
        organisation.SourceSystem.Should().Be("GIAS");
        organisation.ProviderTypeName.Should().Be("Academy");
        organisation.ProviderTypeCode.Should().Be(10);
        organisation.GiasProviderType.Should().Be("Academy converter");
        organisation.PimsProviderType.Should().Be("Academy PIMS");
        organisation.PimsProviderTypeCode.Should().Be(11);
        organisation.PimsStatusName.Should().Be("Open PIMS");
        organisation.PimsStatus.Should().Be(1);
        organisation.GiasStatusName.Should().Be("Open GIAS");
        organisation.GiasStatus.Should().Be(2);
        organisation.MasterProviderStatusName.Should().Be("Open master");
        organisation.MasterProviderStatusCode.Should().Be(3);
        organisation.OpenedOn.Should().Be(new DateTime(2010, 9, 1));
        organisation.DistrictAdministrativeName.Should().Be("Camden district");
        organisation.DistrictAdministrativeCode.Should().Be("E09000007");
        organisation.IsOnApar.Should().Be("Yes");
        organisation.Services.Should().BeEmpty();
        organisation.IsEstablishment.Should().BeTrue();
    }
}
