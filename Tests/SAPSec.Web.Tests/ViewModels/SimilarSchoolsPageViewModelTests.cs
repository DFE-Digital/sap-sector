using FluentAssertions;
using SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;

namespace SAPSec.Web.Tests.ViewModels;

public class SimilarSchoolsPageViewModelTests
{
    [Fact]
    public void HiddenFormFields_WhenViewUrlContainsSecondaryPhase_ReturnsPhaseHiddenField()
    {
        var model = BuildModel("/school/all-through/123456/view-similar-schools?phase=secondary");

        model.HiddenFormFields.Should().ContainSingle()
            .Which.Should().Be(new SimilarSchoolsHiddenFormFieldViewModel("phase", "secondary"));
    }

    [Fact]
    public void HiddenFormFields_WhenViewUrlContainsStoredPrimaryQuery_ReturnsDecodedStoredQueryHiddenField()
    {
        var model = BuildModel("/school/all-through/123456/view-similar-schools?phase=secondary&primaryQuery=%3FsortBy%3DRwmExpected%26reg%3DNorth%2520East");

        model.HiddenFormFields.Should().BeEquivalentTo(
        [
            new SimilarSchoolsHiddenFormFieldViewModel("phase", "secondary"),
            new SimilarSchoolsHiddenFormFieldViewModel("primaryQuery", "?sortBy=RwmExpected&reg=North%20East")
        ], options => options.WithStrictOrdering());
    }

    [Fact]
    public void HiddenFormFields_WhenViewUrlContainsFilterParameters_DoesNotReturnFilterHiddenFields()
    {
        var model = BuildModel("/school/primary/123456/view-similar-schools?reg=London&sortBy=RwmExpected");

        model.HiddenFormFields.Should().BeEmpty();
    }

    private static SimilarSchoolsPageViewModel BuildModel(string viewSimilarSchoolsUrl) =>
        new()
        {
            Urn = "123456",
            SchoolName = "Test school",
            PhaseLabel = "secondary",
            ViewSimilarSchoolsUrl = viewSimilarSchoolsUrl,
            WhatIsASimilarSchoolUrl = "/what-is-a-similar-school",
            SimilarSchools = [],
            MapSchools = [],
            FilterGroups = [],
            SelectedFilterTags = [],
            SortOptions = [],
            ValidationErrors = [],
            CurrentPage = 1,
            PageSize = 10,
            TotalResults = 0,
            SortBy = "",
            CurrentFilters = new(StringComparer.InvariantCultureIgnoreCase)
        };
}
