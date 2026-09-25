using FluentAssertions;
using SAPSec.Web.Areas.AllThrough.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Tests.ViewModels;

public class AllThroughWhatIsASimilarSchoolViewModelTests
{
    private const string Urn = "123456";

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void HasSimilarSchools_WhenAnyPhaseHasSimilarSchools_ReturnsTrue(
        bool hasPrimarySimilarSchools,
        bool hasSecondarySimilarSchools)
    {
        var model = BuildModel(hasPrimarySimilarSchools, hasSecondarySimilarSchools);

        model.HasSimilarSchools.Should().BeTrue();
        model.ViewSimilarSchoolsUrl.Should().Be(Routes.AllThroughSchool(Urn).ViewSimilarSchools);
    }

    [Fact]
    public void HasSimilarSchools_WhenNeitherPhaseHasSimilarSchools_ReturnsFalse()
    {
        var model = BuildModel(false, false);

        model.HasSimilarSchools.Should().BeFalse();
        model.OverviewUrl.Should().Be(Routes.AllThroughSchool(Urn).Overview);
    }

    private static AllThroughWhatIsASimilarSchoolViewModel BuildModel(
        bool hasPrimarySimilarSchools,
        bool hasSecondarySimilarSchools) =>
        new(
            new SchoolInfoViewModel(Urn, "Test school", "Test address"),
            hasPrimarySimilarSchools,
            hasSecondarySimilarSchools);
}
