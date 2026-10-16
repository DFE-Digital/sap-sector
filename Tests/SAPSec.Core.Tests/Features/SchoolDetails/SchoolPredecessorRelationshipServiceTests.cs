using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class SchoolPredecessorRelationshipServiceTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly SchoolPredecessorRelationshipService _sut;

    public SchoolPredecessorRelationshipServiceTests()
    {
        _sut = new SchoolPredecessorRelationshipService(_establishmentRepo, Mock.Of<ILogger<SchoolPredecessorRelationshipService>>());
    }

    [Fact]
    public async Task ClosedSchool_NeverShowsSuccessorBanner()
    {
        // The closed-school banner takes priority - see SchoolClosureEligibilityService.
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeFalse();
        result.Predecessors.Should().BeEmpty();
    }

    [Fact]
    public async Task OpenSchool_WithNoPredecessorLinks_DoesNotShowBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Open());

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeFalse();
    }

    [Fact]
    public async Task OpenSchool_WithSinglePredecessor_ShowsBannerWithOnePredecessor()
    {
        var establishment = Build.Establishment("123456", "New Academy", x => x.Open());
        var predecessor = Build.Establishment("654321", "Old Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment, predecessor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeTrue();
        result.Predecessors.Should().ContainSingle();
        result.Predecessors[0].Urn.Should().Be("654321");
        result.Predecessors[0].Name.Should().Be("Old Academy");
    }

    [Theory]
    [InlineData("Predecessor")]
    [InlineData("Predecessor - amalgamated")]
    [InlineData("Predecessor - merged")]
    public async Task OpenSchool_WithCoveredLinkType_ResolvesPredecessor(string linkType)
    {
        var establishment = Build.Establishment("123456", "New Academy", x => x.Open());
        var predecessor = Build.Establishment("654321", "Old Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment, predecessor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", linkType, "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeTrue();
    }

    [Fact]
    public async Task OpenSchool_WithUncoveredLinkType_DoesNotShowBanner()
    {
        var establishment = Build.Establishment("123456", "New Academy", x => x.Open());
        var predecessor = Build.Establishment("654321", "Old Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment, predecessor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor - Split School", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeFalse();
    }

    [Fact]
    public async Task OpenSchool_WithMultiplePredecessors_ResolvesAllOfThem()
    {
        var establishment = Build.Establishment("123456", "New Academy", x => x.Open());
        var predecessorA = Build.Establishment("111111", "Academy A", x => x.Closed());
        var predecessorB = Build.Establishment("222222", "Academy B", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment, predecessorA, predecessorB)
            .SetupEstablishmentLinks(
                Build.EstablishmentLink("123456", "111111", "Predecessor", "Academy A"),
                Build.EstablishmentLink("123456", "222222", "Predecessor", "Academy B"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeTrue();
        result.Predecessors.Should().HaveCount(2);
        result.Predecessors.Select(p => p.Urn).Should().BeEquivalentTo(["111111", "222222"]);
    }

    [Fact]
    public async Task OpenSchool_WithPredecessorBeyondDataEligibilityWindow_DoesNotLinkToIt()
    {
        var establishment = Build.Establishment("123456", "New Academy", x => x.Open());
        var longAgoCloseDate = DateTime.UtcNow.AddYears(-5).ToString("dd-MM-yyyy");
        var predecessor = Build.Establishment("654321", "Old Academy", x => x.Closed(longAgoCloseDate));

        _establishmentRepo
            .SetupEstablishments(establishment, predecessor)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.ShowSuccessorBanner.Should().BeFalse();
        result.Predecessors.Should().BeEmpty();
    }
}
