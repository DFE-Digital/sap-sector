using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SAPSec.Web.Areas.AllThrough.Services;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Tests.Services;

public class AllThroughSimilarSchoolsTabUrlBuilderTests
{
    private const string Urn = "123456";
    private readonly IAllThroughSimilarSchoolsTabUrlBuilder _sut = new AllThroughSimilarSchoolsTabUrlBuilder();

    [Fact]
    public void Build_WhenPrimarySelectedWithoutQuery_ReturnsBasePrimaryAndSecondaryTabUrls()
    {
        var urls = _sut.Build(Urn, "primary", Query());

        urls.PrimaryTabUrl.Should().Be(Routes.AllThroughSchool(Urn).ViewSimilarSchools);
        urls.SecondaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary");
    }

    [Fact]
    public void Build_WhenPrimarySelectedWithFilters_PreservesCurrentPrimaryQueryOnBothTabs()
    {
        var urls = _sut.Build(
            Urn,
            "primary",
            Query(
                ("sortBy", "RwmExpected"),
                ("reg", "East Midlands"),
                ("phase", "primary"),
                ("focusTarget", "filters")));

        urls.PrimaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?sortBy=RwmExpected&reg=East%20Midlands&focusTarget=filters");
        urls.SecondaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary&primaryQuery=%3FsortBy%3DRwmExpected%26reg%3DEast%2520Midlands%26focusTarget%3Dfilters");
    }

    [Fact]
    public void Build_WhenPrimarySelectedWithStoredSecondaryQuery_RestoresSecondaryTabFilters()
    {
        var urls = _sut.Build(
            Urn,
            "primary",
            Query(
                ("sortBy", "RwmExpected"),
                ("secondaryQuery", "?sortBy=Att8&reg=London")));

        urls.PrimaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?sortBy=RwmExpected&secondaryQuery=%3FsortBy%3DAtt8%26reg%3DLondon");
        urls.SecondaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary&sortBy=Att8&reg=London&primaryQuery=%3FsortBy%3DRwmExpected");
    }

    [Fact]
    public void Build_WhenSecondarySelectedWithFilters_PreservesCurrentSecondaryQueryOnBothTabs()
    {
        var urls = _sut.Build(
            Urn,
            "secondary",
            Query(
                ("phase", "secondary"),
                ("sortBy", "Att8"),
                ("reg", "London"),
                ("primaryQuery", "?sortBy=RwmExpected&reg=North East")));

        urls.PrimaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?sortBy=RwmExpected&reg=North East&secondaryQuery=%3FsortBy%3DAtt8%26reg%3DLondon");
        urls.SecondaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary&sortBy=Att8&reg=London&primaryQuery=%3FsortBy%3DRwmExpected%26reg%3DNorth%20East");
    }

    [Fact]
    public void Build_ExcludesStoredPhaseQueriesFromCurrentPhaseQuery()
    {
        var urls = _sut.Build(
            Urn,
            "primary",
            Query(
                ("primaryQuery", "?sortBy=Ignored"),
                ("secondaryQuery", "?sortBy=Att8"),
                ("phase", "secondary"),
                ("sortBy", "RwmExpected")));

        urls.PrimaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?sortBy=Ignored&secondaryQuery=%3FsortBy%3DAtt8");
        urls.SecondaryTabUrl.Should().Be($"{Routes.AllThroughSchool(Urn).ViewSimilarSchools}?phase=secondary&sortBy=Att8&primaryQuery=%3FsortBy%3DRwmExpected");
    }

    private static QueryCollection Query(params (string Key, string Value)[] values) =>
        new(values.ToDictionary(
            kvp => kvp.Key,
            kvp => new StringValues(kvp.Value)));
}
