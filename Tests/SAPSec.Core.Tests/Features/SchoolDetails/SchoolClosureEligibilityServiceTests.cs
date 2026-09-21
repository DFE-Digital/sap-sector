using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class SchoolClosureEligibilityServiceTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly SchoolClosureEligibilityService _sut;

    public SchoolClosureEligibilityServiceTests()
    {
        _sut = new SchoolClosureEligibilityService(_establishmentRepo, Mock.Of<ILogger<SchoolClosureEligibilityService>>());
    }

    [Fact]
    public async Task OpenSchool_IsEligibleAndDoesNotShowBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Open());

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedSchool_WithUnknownCloseDateAndNoSuccessor_IsEligibleAndShowsBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeTrue();
    }

    [Fact]
    public async Task ClosedSchool_WithinFourYearWindowAndNoSuccessor_IsEligibleAndShowsBanner()
    {
        var recentCloseDate = DateTime.UtcNow.AddYears(-1).ToString("dd-MM-yyyy");
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed(recentCloseDate));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeTrue();
    }

    [Fact]
    public async Task ClosedSchool_BeyondFourYearWindow_IsNotEligibleAndDoesNotShowBanner()
    {
        var longAgoCloseDate = DateTime.UtcNow.AddYears(-5).ToString("dd-MM-yyyy");
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed(longAgoCloseDate));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeFalse();
        result.ShowClosedSchoolBanner.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedSchool_WithSuccessor_IsEligibleButDoesNotShowPlainBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());
        var successor = Build.Establishment("654321", "New Academy", x => x.Open());

        _establishmentRepo
            .SetupEstablishments(establishment, successor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Successor", "New Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedSchool_WithOnlyPredecessorLink_StillShowsPlainBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeTrue();
        result.ShowPredecessorBanner.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedSchool_WithSingleSuccessor_ResolvesPredecessorBannerWithOneSuccessor()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());
        var successor = Build.Establishment("654321", "New Academy", x => x.Open().Secondary());

        _establishmentRepo
            .SetupEstablishments(establishment, successor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Successor", "New Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowPredecessorBanner.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeFalse();
        result.Successors.Should().ContainSingle();
        result.Successors[0].Urn.Should().Be("654321");
        result.Successors[0].Name.Should().Be("New Academy");
    }

    [Fact]
    public async Task ClosedSchool_WithSuccessorAmalgamatedLinkType_ResolvesPredecessorBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());
        var successor = Build.Establishment("654321", "New Academy", x => x.Open());

        _establishmentRepo
            .SetupEstablishments(establishment, successor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Successor - amalgamated", "New Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowPredecessorBanner.Should().BeTrue();
        result.Successors.Should().ContainSingle(s => s.Urn == "654321");
    }

    [Fact]
    public async Task ClosedSchool_WithMultipleSuccessors_ResolvesAllOfThem()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());
        var successorA = Build.Establishment("111111", "Academy A", x => x.Open());
        var successorB = Build.Establishment("222222", "Academy B", x => x.Open());

        _establishmentRepo
            .SetupEstablishments(establishment, successorA, successorB)
            .SetupEstablishmentLinks(
                Build.EstablishmentLink("123456", "111111", "Successor", "Academy A"),
                Build.EstablishmentLink("123456", "222222", "Successor", "Academy B"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowPredecessorBanner.Should().BeTrue();
        result.Successors.Should().HaveCount(2);
        result.Successors.Select(s => s.Urn).Should().BeEquivalentTo(["111111", "222222"]);
    }

    [Fact]
    public async Task ClosedSchool_ResultOfAmalgamationWhereLinkedSchoolIsOpen_TreatsCurrentSchoolAsPredecessor()
    {
        // Mirrors the real GIAS data shape: a closed school's own row points to the open,
        // newly-amalgamated school via "Result of Amalgamation".
        var establishment = Build.Establishment("123456", "Old Academy", x => x.Closed());
        var successor = Build.Establishment("654321", "Amalgamated Academy", x => x.Open());

        _establishmentRepo
            .SetupEstablishments(establishment, successor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Result of Amalgamation", "Amalgamated Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowPredecessorBanner.Should().BeTrue();
        result.Successors.Should().ContainSingle(s => s.Urn == "654321");
    }

    [Fact]
    public async Task ClosedSchool_ResultOfAmalgamationWhereLinkedSchoolIsAlsoClosed_DoesNotResolveSuccessorButSuppressesPlainBanner()
    {
        // The unexpected-combination case flagged in the ticket: both sides of the amalgamation
        // link are closed, so there is no valid destination to signpost to.
        var establishment = Build.Establishment("123456", "Academy One", x => x.Closed());
        var otherClosedSchool = Build.Establishment("654321", "Academy Two", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment, otherClosedSchool)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Result of Amalgamation", "Academy Two"));

        var result = await _sut.EvaluateAsync(establishment);

        result.Successors.Should().BeEmpty();
        result.ShowPredecessorBanner.Should().BeFalse();
        result.ShowClosedSchoolBanner.Should().BeFalse();
    }
}
