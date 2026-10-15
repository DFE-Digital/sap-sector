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
    [InlineData("0", true, true, false)]
    [InlineData("2", true, false, true)]
    [InlineData("2", false, true, false)]
    [InlineData("4", false, false, true)]
    [InlineData("7", false, true, true)]
    [InlineData("7", true, false, false)]
    public void CanSearch_UsesPhaseIdAndFeatureFlags(
        string phaseId,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled,
        bool expected)
    {
        var result = new Establishment
        {
            PhaseOfEducationId = phaseId,
            EstablishmentStatusId = expected ? "1" : string.Empty
        }.CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("3", true)]
    [InlineData("2", true)] // Closed with no known close date is treated as within the eligibility window
    [InlineData("4", false)] // Proposed to open is unrelated to closure eligibility and always excluded
    public void CanSearch_UsesStatusId(string statusId, bool expected)
    {
        var result = new Establishment { PhaseOfEducationId = "4", EstablishmentStatusId = statusId }
            .CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "2", true)] // Closed, no known close date -> still eligible
    [InlineData(true, "4", false)] // Proposed to open -> always excluded
    [InlineData(false, "2", true)]
    [InlineData(false, "4", false)]
    public void CanSearch_WithSecondaryPhaseNameAndStatusId(
        bool primarySchoolsEnabled,
        string statusId,
        bool expected)
    {
        var result = new Establishment
        {
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusId = statusId
        }.CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled: true);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Open", true)]
    [InlineData("Open, but proposed to close", true)]
    [InlineData("Closed", true)] // No known close date -> still eligible
    [InlineData("Proposed to open", false)]
    public void CanSearch_FallsBackToStatusName(string statusName, bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = "Secondary", EstablishmentStatusName = statusName }
            .CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Fact]
    public void CanSearch_ClosedSchoolWithinEligibilityWindow_ReturnsTrue()
    {
        var recentCloseDate = DateTime.UtcNow.AddYears(-1).ToString("dd-MM-yyyy");

        var result = new Establishment
        {
            PhaseOfEducationId = "4",
            EstablishmentStatusId = "2",
            CloseDate = recentCloseDate
        }.CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanSearch_ClosedSchoolBeyondEligibilityWindow_ReturnsFalse()
    {
        var longAgoCloseDate = DateTime.UtcNow.AddYears(-5).ToString("dd-MM-yyyy");

        var result = new Establishment
        {
            PhaseOfEducationId = "4",
            EstablishmentStatusId = "2",
            CloseDate = longAgoCloseDate
        }.CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Primary", false, false, false)]
    [InlineData("Primary", true, false, true)]
    [InlineData("All-through", false, false, false)]
    [InlineData("All-through", false, true, true)]
    [InlineData("All-through", true, false, false)]
    [InlineData("Secondary", false, false, true)]
    public void CanSearch_FallsBackToPhaseName(
        string phaseName,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled,
        bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = phaseName, EstablishmentStatusId = "1" }
            .CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled);

        result.Should().Be(expected);
    }

    [Fact]
    public void CanSearch_WithSecondarySchoolAndMissingStatus_ReturnsTrue()
    {
        var result = new Establishment { PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" }
            .CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndMissingStatus_ReturnsFalse()
    {
        var result = new Establishment { PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" }
            .CanSearch(primarySchoolsEnabled: true, allThroughSchoolsEnabled: true);

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
        }.CanSearch(primarySchoolsEnabled: true, allThroughSchoolsEnabled: false);

        result.Should().BeTrue();
    }
}
