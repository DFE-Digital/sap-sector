using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Tests.Features.SchoolSearch.Extensions;

public class SchoolTests
{
    [Theory]
    [InlineData(PhaseOfEducationId.PrimaryId)]
    [InlineData(PhaseOfEducationId.SecondaryId)]
    [InlineData(PhaseOfEducationId.AllThroughId)]
    public void CanIndexForSearch_WithSupportedPhaseIds_ReturnsTrue(string phaseId)
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = phaseId,
        });

        school.IsSearchable(primarySchoolsEnabled: true, allThroughSchoolsEnabled: true).Should().BeTrue();
    }

    [Theory]
    [InlineData(PhaseOfEducationId.NotApplicableId)]
    [InlineData(PhaseOfEducationId.NurseryId)]
    [InlineData(PhaseOfEducationId.MiddleDeemedPrimaryId)]
    [InlineData(PhaseOfEducationId.MiddleDeemedSecondaryId)]
    [InlineData(PhaseOfEducationId.SixteenPlusId)]
    public void CanIndexForSearch_WithUnsupportedPhaseIds_ReturnsFalse(string phaseId)
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = phaseId,
        });

        school.IsSearchable(primarySchoolsEnabled: true, allThroughSchoolsEnabled: true).Should().BeFalse();
    }

    //[Fact]
    //public void CanIndexForSearch_WithLegacyAllThroughName_ReturnsTrue()
    //{
    //    var result = new Establishment
    //    {
    //        PhaseOfEducationId = "All-through"
    //    }.CanIndexForSearch();

    //    result.Should().BeTrue();
    //}

    [Theory]
    [InlineData(PhaseOfEducationId.NotApplicableId, true, true, false)]
    [InlineData(PhaseOfEducationId.PrimaryId, true, false, true)]
    [InlineData(PhaseOfEducationId.PrimaryId, false, true, false)]
    [InlineData(PhaseOfEducationId.SecondaryId, false, false, true)]
    [InlineData(PhaseOfEducationId.AllThroughId, false, true, true)]
    [InlineData(PhaseOfEducationId.AllThroughId, true, false, false)]
    public void CanSearch_UsesPhaseIdAndFeatureFlags(
        string phaseId,
        bool primarySchoolsEnabled,
        bool allThroughSchoolsEnabled,
        bool expected)
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = phaseId,
            EstablishmentStatusId = expected ? "1" : string.Empty
        });

        school.IsSearchable(primarySchoolsEnabled, allThroughSchoolsEnabled).Should().Be(expected);
    }

    [Theory]
    [InlineData(EstablishmentStatusId.OpenId, true)]
    [InlineData(EstablishmentStatusId.OpenButProposedToCloseId, true)]
    [InlineData(EstablishmentStatusId.ClosedId, true)] // Closed with no known close date is treated as within the eligibility window
    [InlineData(EstablishmentStatusId.ProposedToOpenId, false)] // Proposed to open is unrelated to closure eligibility and always excluded
    public void CanSearch_UsesStatusId(string statusId, bool expected)
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = "4",
            EstablishmentStatusId = statusId
        });

        school.IsSearchable(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, EstablishmentStatusId.ClosedId, true)] // Closed, no known close date -> still eligible
    [InlineData(true, EstablishmentStatusId.ProposedToOpenId, false)] // Proposed to open -> always excluded
    [InlineData(false, EstablishmentStatusId.ClosedId, true)]
    [InlineData(false, EstablishmentStatusId.ProposedToOpenId, false)]
    public void CanSearch_WithSecondaryPhaseNameAndStatusId(
        bool primarySchoolsEnabled,
        string statusId,
        bool expected)
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationName = "Secondary",
            EstablishmentStatusId = statusId
        });

        school.IsSearchable(primarySchoolsEnabled, allThroughSchoolsEnabled: true).Should().Be(expected);
    }

    //[Theory]
    //[InlineData("Open", true)]
    //[InlineData("Open, but proposed to close", true)]
    //[InlineData("Closed", true)] // No known close date -> still eligible
    //[InlineData("Proposed to open", false)]
    //public void CanSearch_FallsBackToStatusName(string statusName, bool expected)
    //{
    //    var result = new Establishment 
    //    { 
    //        PhaseOfEducationName = "Secondary", 
    //        EstablishmentStatusName = statusName 
    //    }
    //        .CanSearch(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false);

    //    result.Should().Be(expected);
    //}

    [Fact]
    public void CanSearch_ClosedSchoolWithinEligibilityWindow_ReturnsTrue()
    {
        var recentCloseDate = DateTime.UtcNow.AddYears(-1).ToString("dd-MM-yyyy");

        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = PhaseOfEducationId.SecondaryId,
            EstablishmentStatusId = EstablishmentStatusId.ClosedId,
            CloseDate = recentCloseDate
        });

        school.IsSearchable(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false).Should().BeTrue();
    }

    [Fact]
    public void CanSearch_ClosedSchoolBeyondEligibilityWindow_ReturnsFalse()
    {
        var longAgoCloseDate = DateTime.UtcNow.AddYears(-5).ToString("dd-MM-yyyy");

        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = "4",
            EstablishmentStatusId = "2",
            CloseDate = longAgoCloseDate
        });

        school.IsSearchable(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false).Should().BeFalse();
    }

    //[Theory]
    //[InlineData("Primary", false, false, false)]
    //[InlineData("Primary", true, false, true)]
    //[InlineData("All-through", false, false, false)]
    //[InlineData("All-through", false, true, true)]
    //[InlineData("All-through", true, false, false)]
    //[InlineData("Secondary", false, false, true)]
    //public void CanSearch_FallsBackToPhaseName(
    //    string phaseName,
    //    bool primarySchoolsEnabled,
    //    bool allThroughSchoolsEnabled,
    //    bool expected)
    //{
    //    var result = new Establishment { PhaseOfEducationName = phaseName, EstablishmentStatusId = "1" }
    //        .CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled);

    //    result.Should().Be(expected);
    //}

    [Fact]
    public void CanSearch_WithSecondarySchoolAndMissingStatus_ReturnsTrue()
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = PhaseOfEducationId.SecondaryId
        });

        school.IsSearchable(primarySchoolsEnabled: false, allThroughSchoolsEnabled: false).Should().BeTrue();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndMissingStatus_ReturnsFalse()
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = PhaseOfEducationId.PrimaryId
        });

        school.IsSearchable(primarySchoolsEnabled: true, allThroughSchoolsEnabled: true).Should().BeFalse();
    }

    [Fact]
    public void CanSearch_WithPrimarySchoolAndEstablishmentStatus_ReturnsTrue()
    {
        var school = School.Create(new Establishment
        {
            PhaseOfEducationId = "2",
            PhaseOfEducationName = "Primary",
            EstablishmentStatusId = "1"
        });

        school.IsSearchable(primarySchoolsEnabled: true, allThroughSchoolsEnabled: false).Should().BeTrue();
    }
}
