using FluentAssertions;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.ViewModels;

public class SchoolSideNavigationViewModelTests
{
    private const string Urn = "123456";

    [Theory]
    [MemberData(nameof(AllThroughNavigationScenarios))]
    public void CreateAllThrough_ReturnsExpectedItems(
        bool includeSimilarSchools,
        bool includeRiseResources,
        string[] expectedText,
        string[] expectedHref)
    {
        var model = SchoolSideNavigationViewModel.CreateAllThrough(
            url: null!,
            urn: Urn,
            currentAction: "Index",
            includeSimilarSchools,
            includeRiseResources);

        model.Items.Select(x => x.Text).Should().Equal(expectedText);
        model.Items.Select(x => x.Href).Should().Equal(expectedHref);
        model.Items.Should().ContainSingle(x => x.Text == "Overview" && x.IsSelected);
        model.Items.Should().OnlyContain(x => x.Href.StartsWith(Routes.AllThroughSchool(Urn).Overview));
    }

    [Theory]
    [InlineData("Index", "Overview")]
    [InlineData("Ks2PerformanceMeasures", "KS2")]
    [InlineData("Ks4HeadlineMeasures", "KS4 headline measures")]
    [InlineData("Ks4CoreSubjects", "KS4 core subjects")]
    [InlineData("Attendance", "Attendance")]
    [InlineData("ViewSimilarSchools", "View similar schools")]
    [InlineData("SchoolDetails", "School details")]
    [InlineData("WhatIsASimilarSchool", "What is a similar school?")]
    [InlineData("RiseResources", "RISE resources")]
    public void CreateAllThrough_SelectsCurrentAction(string currentAction, string expectedSelectedItem)
    {
        var model = SchoolSideNavigationViewModel.CreateAllThrough(
            url: null!,
            urn: Urn,
            currentAction,
            includeSimilarSchools: true,
            includeRiseResources: true);

        model.Items.Should().ContainSingle(x => x.IsSelected)
            .Which.Text.Should().Be(expectedSelectedItem);
    }

    [Fact]
    public void CreatePrimary_ReturnsExistingPrimaryItems()
    {
        var model = SchoolSideNavigationViewModel.CreatePrimary(
            url: null!,
            urn: Urn,
            currentAction: "Index",
            includeRiseResources: true);

        model.Items.Select(x => x.Text).Should().Equal(
            "Overview",
            "KS2",
            "Attendance",
            "View similar schools",
            "School details",
            "What is a similar school?",
            "RISE resources");

        model.Items.Select(x => x.Href).Should().Equal(
            Routes.PrimarySchool(Urn).Overview,
            Routes.PrimarySchool(Urn).KS2,
            Routes.PrimarySchool(Urn).Attendance,
            Routes.PrimarySchool(Urn).ViewSimilarSchools,
            Routes.PrimarySchool(Urn).SchoolDetails,
            Routes.PrimarySchool(Urn).WhatIsASimilarSchool,
            Routes.PrimarySchool(Urn).RiseResources);
    }

    [Fact]
    public void CreateSecondary_ReturnsExistingSecondaryItems()
    {
        var model = SchoolSideNavigationViewModel.CreateSecondary(
            url: null!,
            urn: Urn,
            currentAction: "Index",
            includeRiseResources: true);

        model.Items.Select(x => x.Text).Should().Equal(
            "Overview",
            "KS4 headline measures",
            "KS4 core subjects",
            "Attendance",
            "View similar schools",
            "School details",
            "What is a similar school?",
            "RISE resources");

        model.Items.Select(x => x.Href).Should().Equal(
            Routes.SecondarySchool(Urn).Overview,
            Routes.SecondarySchool(Urn).KS4HeadlineMeasures,
            Routes.SecondarySchool(Urn).KS4CoreSubjects,
            Routes.SecondarySchool(Urn).Attendance,
            Routes.SecondarySchool(Urn).ViewSimilarSchools,
            Routes.SecondarySchool(Urn).SchoolDetails,
            Routes.SecondarySchool(Urn).WhatIsASimilarSchool,
            Routes.SecondarySchool(Urn).RiseResources);
    }

    public static TheoryData<bool, bool, string[], string[]> AllThroughNavigationScenarios => new()
    {
        {
            true,
            true,
            [
                "Overview",
                "KS2",
                "KS4 headline measures",
                "KS4 core subjects",
                "Attendance",
                "View similar schools",
                "School details",
                "What is a similar school?",
                "RISE resources"
            ],
            [
                Routes.AllThroughSchool(Urn).Overview,
                Routes.AllThroughSchool(Urn).KS2,
                Routes.AllThroughSchool(Urn).KS4HeadlineMeasures,
                Routes.AllThroughSchool(Urn).KS4CoreSubjects,
                Routes.AllThroughSchool(Urn).Attendance,
                Routes.AllThroughSchool(Urn).ViewSimilarSchools,
                Routes.AllThroughSchool(Urn).SchoolDetails,
                Routes.AllThroughSchool(Urn).WhatIsASimilarSchool,
                Routes.AllThroughSchool(Urn).RiseResources
            ]
        },
        {
            false,
            true,
            [
                "Overview",
                "KS2",
                "KS4 headline measures",
                "KS4 core subjects",
                "Attendance",
                "School details",
                "What is a similar school?",
                "RISE resources"
            ],
            [
                Routes.AllThroughSchool(Urn).Overview,
                Routes.AllThroughSchool(Urn).KS2,
                Routes.AllThroughSchool(Urn).KS4HeadlineMeasures,
                Routes.AllThroughSchool(Urn).KS4CoreSubjects,
                Routes.AllThroughSchool(Urn).Attendance,
                Routes.AllThroughSchool(Urn).SchoolDetails,
                Routes.AllThroughSchool(Urn).WhatIsASimilarSchool,
                Routes.AllThroughSchool(Urn).RiseResources
            ]
        }
    };
}
