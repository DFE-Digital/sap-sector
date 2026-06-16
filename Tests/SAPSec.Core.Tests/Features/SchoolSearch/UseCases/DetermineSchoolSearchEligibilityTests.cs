using SAPSec.Core.Features.SchoolSearch.UseCases;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.SchoolSearch.UseCases;

public class DetermineSchoolSearchEligibilityTests
{
    private readonly DetermineSchoolSearchEligibility _sut = new();

    [Theory]
    [InlineData("2", "Primary")]
    [InlineData("4", "Secondary")]
    [InlineData("7", "All-through")]
    public void CanIndex_WithSupportedPhaseIds_ReturnsTrue(string phaseId, string phaseName)
    {
        var result = _sut.CanIndex(new Establishment
        {
            PhaseOfEducationId = phaseId,
            PhaseOfEducationName = phaseName
        });

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("0", "Not applicable")]
    [InlineData("1", "Nursery")]
    [InlineData("3", "Middle deemed primary")]
    [InlineData("5", "Middle deemed secondary")]
    [InlineData("6", "16 plus")]
    public void CanIndex_WithUnsupportedPhaseIds_ReturnsFalse(string phaseId, string phaseName)
    {
        var result = _sut.CanIndex(new Establishment
        {
            PhaseOfEducationId = phaseId,
            PhaseOfEducationName = phaseName
        });

        result.Should().BeFalse();
    }

    [Fact]
    public void CanIndex_WithLegacyAllThroughName_ReturnsTrue()
    {
        var result = _sut.CanIndex(new Establishment
        {
            PhaseOfEducationName = "All-through"
        });

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
        var result = _sut.CanSearch(
            new Establishment
            {
                PhaseOfEducationId = phaseId,
                EstablishmentStatusId = expected ? "1" : string.Empty
            },
            primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("3", true)]
    [InlineData("2", false)]
    [InlineData("4", false)]
    public void CanSearch_UsesStatusId(string statusId, bool expected)
    {
        var result = _sut.CanSearch(
            new Establishment { PhaseOfEducationId = "4", EstablishmentStatusId = statusId },
            primarySchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Open", true)]
    [InlineData("Open, but proposed to close", true)]
    [InlineData("Closed", false)]
    [InlineData("Proposed to open", false)]
    public void CanSearch_FallsBackToStatusName(string statusName, bool expected)
    {
        var result = _sut.CanSearch(
            new Establishment { PhaseOfEducationName = "Secondary", EstablishmentStatusName = statusName },
            primarySchoolsEnabled: false);

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
        var result = _sut.CanSearch(
            new Establishment { PhaseOfEducationName = phaseName, EstablishmentStatusId = "1" },
            primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Fact]
    public void CanSearch_WithSecondarySchoolAndMissingStatus_ReturnsTrue()
    {
        var result = _sut.CanSearch(
            new Establishment { PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" },
            primarySchoolsEnabled: false);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndMissingStatus_ReturnsFalse()
    {
        var result = _sut.CanSearch(
            new Establishment { PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" },
            primarySchoolsEnabled: true);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndEstablishmentStatus_ReturnsTrue()
    {
        var result = _sut.CanSearch(
            new Establishment
            {
                PhaseOfEducationId = "2",
                PhaseOfEducationName = "Primary",
                EstablishmentStatusId = "1"
            },
            primarySchoolsEnabled: true);

        result.Should().BeTrue();
    }
}
