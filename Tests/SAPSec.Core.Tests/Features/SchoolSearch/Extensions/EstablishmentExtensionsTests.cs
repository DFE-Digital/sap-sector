using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Features.SchoolSearch.Extensions;

public class EstablishmentExtensionsTests
{
    [Theory]
    [InlineData("2", "Primary")]
    [InlineData("4", "Secondary")]
    [InlineData("7", "All-through")]
    public void CanIndexForSearch_WithSupportedPhaseIds_ReturnsTrue(string phaseId, string phaseName)
    {
        var result = new Establishment
        {
            PhaseOfEducationId = phaseId,
            PhaseOfEducationName = phaseName
        }.CanIndexForSearch();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("0", "Not applicable")]
    [InlineData("1", "Nursery")]
    [InlineData("3", "Middle deemed primary")]
    [InlineData("5", "Middle deemed secondary")]
    [InlineData("6", "16 plus")]
    public void CanIndexForSearch_WithUnsupportedPhaseIds_ReturnsFalse(string phaseId, string phaseName)
    {
        var result = new Establishment
        {
            PhaseOfEducationId = phaseId,
            PhaseOfEducationName = phaseName
        }.CanIndexForSearch();

        result.Should().BeFalse();
    }

    [Fact]
    public void CanIndexForSearch_WithLegacyAllThroughName_ReturnsTrue()
    {
        var result = new Establishment
        {
            PhaseOfEducationName = "All-through"
        }.CanIndexForSearch();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("0", true, false)]
    [InlineData("2", true, true)]
    [InlineData("2", false, false)]
    [InlineData("4", true, true)]
    [InlineData("4", false, true)]
    [InlineData("7", true, true)]
    [InlineData("7", false, false)]
    public void CanSearch_UsesPhaseIdAndFeatureFlag(string phaseId, bool primarySchoolsEnabled, bool expected)
    {
        var result = new Establishment
        {
            PhaseOfEducationId = phaseId,
            EstablishmentStatusId = expected ? "1" : string.Empty
        }.CanSearch(primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("3", true)]
    [InlineData("2", false)]
    [InlineData("4", false)]
    public void CanSearch_UsesStatusId(string statusId, bool expected)
    {
        var result = new Establishment { PhaseOfEducationId = "4", EstablishmentStatusId = statusId }
            .CanSearch(primarySchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "2")]
    [InlineData(true, "4")]
    [InlineData(false, "2")]
    [InlineData(false, "4")]
    public void CanSearch_WithSecondaryPhaseNameAndExcludedStatusId_ReturnsFalse(
        bool primarySchoolsEnabled,
        string statusId)
    {
        var result = new Establishment
        {
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusId = statusId
        }.CanSearch(primarySchoolsEnabled);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Open", true)]
    [InlineData("Open, but proposed to close", true)]
    [InlineData("Closed", false)]
    [InlineData("Proposed to open", false)]
    public void CanSearch_FallsBackToStatusName(string statusName, bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = "Secondary", EstablishmentStatusName = statusName }
            .CanSearch(primarySchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Primary", false, false)]
    [InlineData("Primary", true, true)]
    [InlineData("All-through", false, false)]
    [InlineData("All-through", true, true)]
    [InlineData("Secondary", false, true)]
    public void CanSearch_FallsBackToPhaseName(string phaseName, bool primarySchoolsEnabled, bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = phaseName, EstablishmentStatusId = "1" }
            .CanSearch(primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Fact]
    public void CanSearch_WithSecondarySchoolAndMissingStatus_ReturnsTrue()
    {
        var result = new Establishment { PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" }
            .CanSearch(primarySchoolsEnabled: false);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndMissingStatus_ReturnsFalse()
    {
        var result = new Establishment { PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" }
            .CanSearch(primarySchoolsEnabled: true);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndEstablishmentStatus_ReturnsTrue()
    {
        var result = new Establishment
        {
            PhaseOfEducationId = "2",
            PhaseOfEducationName = "Primary",
            EstablishmentStatusId = "1"
        }.CanSearch(primarySchoolsEnabled: true);

        result.Should().BeTrue();
    }
}
