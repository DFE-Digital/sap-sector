using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.AssociatedSchools;
using SAPSec.Data.Repositories;
using SAPSec.Test.Common.Builders;

namespace SAPSec.Core.Tests.Features.AssociatedSchools;

public class AssociatedSchoolsServiceTests
{
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();
    private readonly AssociatedSchoolsService _sut;

    public AssociatedSchoolsServiceTests()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnablePrimarySchools))
            .ReturnsAsync(true);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(true);

        _sut = new AssociatedSchoolsService(
            _establishmentRepositoryMock.Object,
            _featureFlagServiceMock.Object);
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust)]
    public async Task GetAssociatedSchoolsAsync_WhenTrust_ReturnsSchoolsInTrustOrderedByName(string categoryId)
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsByTrustIdAsync("5001"))
            .ReturnsAsync([
                Build.Establishment("100002", "Zebra Academy", x => x.Open().Secondary().WithTrust("5001")),
                Build.Establishment("100001", "Apple Academy", x => x.Open().Primary().WithTrust("5001"))
            ]);

        var result = await _sut.GetAssociatedSchoolsAsync(Organisation(categoryId, uid: "5001"));

        result.Select(r => r.URN).Should().Equal("100001", "100002");
        _establishmentRepositoryMock.Verify(x => x.GetEstablishmentsByLocalAuthorityIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAssociatedSchoolsAsync_WhenLocalAuthority_ReturnsSchoolsInLocalAuthority()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsByLocalAuthorityIdAsync("210"))
            .ReturnsAsync([
                Build.Establishment("100001", "Southwark School", x => x.Open().Secondary().InLA("210", "Southwark"))
            ]);

        var result = await _sut.GetAssociatedSchoolsAsync(
            Organisation(OrganisationCategoryIds.LocalAuthority, establishmentNumber: "210"));

        result.Should().ContainSingle()
            .Which.LANAme.Should().Be("Southwark");
        _establishmentRepositoryMock.Verify(x => x.GetEstablishmentsByTrustIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAssociatedSchoolsAsync_ExcludesSchoolsThatCannotBeShown()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnablePrimarySchools))
            .ReturnsAsync(false);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsByTrustIdAsync("5001"))
            .ReturnsAsync([
                Build.Establishment("100001", "Primary Academy", x => x.Open().Primary().WithTrust("5001")),
                Build.Establishment("100002", "Secondary Academy", x => x.Open().Secondary().WithTrust("5001"))
            ]);

        var result = await _sut.GetAssociatedSchoolsAsync(Organisation(OrganisationCategoryIds.MultiAcademyTrust, uid: "5001"));

        result.Select(r => r.URN).Should().Equal("100002");
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.Establishment, "5001", "210")]
    [InlineData("011", "5001", "210")]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust, null, "210")]
    [InlineData(OrganisationCategoryIds.LocalAuthority, "5001", null)]
    public async Task GetAssociatedSchoolsAsync_WhenNoTrustOrLocalAuthorityIdentifier_ReturnsNoSchools(
        string categoryId, string? uid, string? establishmentNumber)
    {
        var result = await _sut.GetAssociatedSchoolsAsync(Organisation(categoryId, uid, establishmentNumber));

        result.Should().BeEmpty();
        _establishmentRepositoryMock.VerifyNoOtherCalls();
    }

    private static Organisation Organisation(string categoryId, string? uid = null, string? establishmentNumber = null) =>
        new()
        {
            Id = "org-1",
            Name = "Test Organisation",
            Category = new Category { Id = categoryId },
            Uid = uid,
            EstablishmentNumber = establishmentNumber
        };
}
