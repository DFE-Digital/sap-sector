using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.SchoolSearch.Extensions;

public class EstablishmentExtensionsTests
{
    [Theory]
    [InlineData("0", true, false)]
    [InlineData("2", true, true)]
    [InlineData("2", false, false)]
    [InlineData("4", true, true)]
    [InlineData("4", false, true)]
    [InlineData("7", true, true)]
    [InlineData("7", false, false)]
    public void IsSearchable_UsesPhaseIdAndFeatureFlag(string phaseId, bool primarySchoolsEnabled, bool expected)
    {
        var result = new Establishment
        {
            PhaseOfEducationId = phaseId,
            EstablishmentStatusId = expected ? "1" : string.Empty
        }.IsSearchable(primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("3", true)]
    [InlineData("2", false)]
    [InlineData("4", false)]
    public void IsSearchable_UsesStatusId(string statusId, bool expected)
    {
        var result = new Establishment { PhaseOfEducationId = "4", EstablishmentStatusId = statusId }
            .IsSearchable(primarySchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Open", true)]
    [InlineData("Open, but proposed to close", true)]
    [InlineData("Closed", false)]
    [InlineData("Proposed to open", false)]
    public void IsSearchable_FallsBackToStatusName(string statusName, bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = "Secondary", EstablishmentStatusName = statusName }
            .IsSearchable(primarySchoolsEnabled: false);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Primary", false, false)]
    [InlineData("Primary", true, true)]
    [InlineData("All-through", false, false)]
    [InlineData("All-through", true, true)]
    [InlineData("Secondary", false, true)]
    public void IsSearchable_FallsBackToPhaseName(string phaseName, bool primarySchoolsEnabled, bool expected)
    {
        var result = new Establishment { PhaseOfEducationName = phaseName, EstablishmentStatusId = "1" }
            .IsSearchable(primarySchoolsEnabled);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsSearchable_WhenMissingStatus_AndSecondarySchool_ReturnsFalse()
    {
        var result = new Establishment { PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" }
            .IsSearchable(primarySchoolsEnabled: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSearchable_WhenMissingStatus_AndPrimarySchool_AndFeatureFlagDisabled_ReturnsFalse()
    {
        var result = new Establishment { PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" }
            .IsSearchable(primarySchoolsEnabled: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSearchable_WhenMissingStatus_AndPrimarySchool_AndPrimarySchoolsFeatureFlagEnabled_ReturnsFalse()
    {
        var result = new Establishment { PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" }
            .IsSearchable(primarySchoolsEnabled: true);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSearchable_WhenClosedSchool_ReturnsFalse()
    {
        var result = new Establishment
        {
            PhaseOfEducationId = "4",
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusId = "2"
        }.IsSearchable(primarySchoolsEnabled: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSearchable_WhenClosedSchool_FallingBackToStatusName_ReturnsFalse()
    {
        var result = new Establishment
        {
            PhaseOfEducationId = "4",
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusName = " Closed "
        }.IsSearchable(primarySchoolsEnabled: false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSearchable_WhenOpenSchool_FallingBackToStatusName_ReturnsTrue()
    {
        var result = new Establishment
        {
            PhaseOfEducationId = "4",
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusName = "  Open "
        }.IsSearchable(primarySchoolsEnabled: false);

        result.Should().BeTrue();
    }
}
