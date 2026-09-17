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
        _sut = new SchoolClosureEligibilityService(_establishmentRepo);
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
    public async Task ClosedSchool_WithSuccessor_IsEligibleButDoesNotShowBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Successor", "New Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeFalse();
    }

    [Fact]
    public async Task ClosedSchool_WithOnlyPredecessorLink_StillShowsBanner()
    {
        var establishment = Build.Establishment("123456", "Test Academy", x => x.Closed());

        _establishmentRepo
            .SetupEstablishments(establishment)
            .SetupEstablishmentLinks(Build.EstablishmentLink("123456", "654321", "Predecessor", "Old Academy"));

        var result = await _sut.EvaluateAsync(establishment);

        result.IsEligibleForDisplay.Should().BeTrue();
        result.ShowClosedSchoolBanner.Should().BeTrue();
    }
}
