using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class RiseResourcesPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary().InLA("001")));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnableRiseResources);

        return base.DisposeAsync();
    }

    private static RiseResourceEntry Entry(string title, string category, string subCategory, params string[] phases) =>
        new()
        {
            ResourceTitle = title,
            ResourceDescription = $"{title} description",
            ResourceUrl = $"https://example.gov.uk/{title.Replace(' ', '-').ToLowerInvariant()}",
            Category = category,
            SubCategory = subCategory,
            SchoolPhases = phases
        };

    private static RiseResourceCategoryEntry Category(string name, string description) =>
        new() { Category = name, CategoryDescription = description };

    [Fact]
    public async Task RiseResources_WhenEnableRiseResourcesFeatureFlagEnabled_RendersHeaderAndIntro()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        page.QuerySelector(".govuk-caption-xl")!.TrimmedTextContent().Should().Be("Test School 1");
        page.QuerySelector("h1.govuk-heading-xl")!.TrimmedTextContent().Should().Be(PageTitles.RiseResources);
        page.ElementWithTestIdShouldExist("rise-resources-intro")
            .TrimmedTextContent()
            .Should().Be("Use these resources from RISE to help improve your school’s performance.");
    }

    [Fact]
    public async Task RiseResources_WhenEnableRiseResourcesFeatureFlagDisabled_ReturnsNotFound()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, false);

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("100001").RiseResources);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RiseResources_WithNonExistentUrn_ReturnsNotFound()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        var response = await Fixture.Client.GetAsync(Routes.SecondarySchool("999999").RiseResources);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RiseResources_GroupsByCategoryThenSubCategory_WithContentsLinksDescriptionsAndAlphabeticalResources()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupCategories(
            Category("Performance and attendance", "About performance and attendance."),
            Category("Wider school", "About the wider school."));
        Fixture.RiseResourcesRepository.SetupResources(
            Entry("Teaching maths fluency", "Wider school", "Curriculum and teaching", PhaseOfEducationValues.Secondary),
            Entry("Reading House", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Improving Literacy in KS2", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Improving attendance", "Performance and attendance", "Attendance", PhaseOfEducationValues.Secondary, "All through"),
            Entry("Pastoral support", "Pupil characteristics", "SEND", PhaseOfEducationValues.Secondary),
            Entry("Phonics screening support", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary));

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        page.QuerySelectorAll("[data-testid='rise-resources-category']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("Wider school", "Performance and attendance", "Pupil characteristics");

        page.QuerySelectorAll("[data-testid='rise-resources-category-description']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("About the wider school.", "About performance and attendance.");

        var subCategoryHeadings = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']");
        subCategoryHeadings.Select(el => el.TrimmedTextContent())
            .Should().Equal("Curriculum and teaching", "Literacy", "Attendance", "SEND");
        subCategoryHeadings[0].GetAttribute("id").Should().Be("curriculum-and-teaching");
        subCategoryHeadings[0].GetAttribute("tabindex").Should().Be("-1");

        var contents = page.ElementWithTestIdShouldExist("rise-resources-contents");
        contents.ClassList.Should().Contain("gem-c-contents-list");
        contents.QuerySelector("h2.gem-c-contents-list__title")!.TrimmedTextContent().Should().Be("Contents");
        contents.QuerySelectorAll(".gem-c-contents-list__list > li").Should().OnlyContain(
            li => li.ClassList.Contains("gem-c-contents-list__list-item--dashed"));
        contents.QuerySelectorAll("[aria-hidden='true']").Should().OnlyContain(
            dash => dash.ClassList.Contains("gem-c-contents-list__list-item-dash"));
        contents.QuerySelectorAll("a").Select(a => a.TrimmedTextContent())
            .Should().Equal("Curriculum and teaching", "Literacy", "Attendance", "SEND");
        contents.QuerySelector("a")!.GetAttribute("href").Should().Be("#curriculum-and-teaching");

        var literacyList = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .First(heading => heading.TrimmedTextContent() == "Literacy")
            .NextElementSibling;
        literacyList!.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("Improving Literacy in KS2", "Reading House");

        var link = literacyList.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("https://example.gov.uk/improving-literacy-in-ks2");
    }

    [Fact]
    public async Task RiseResources_OrdersSubCategorySectionsAndContentsByFirstAppearanceInResourceEntries()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupCategories(
            Category("Performance and attendance", "About performance."),
            Category("Wider school", "About the wider school."));
        Fixture.RiseResourcesRepository.SetupResources(
            Entry("Maths guide", "Performance and attendance", "Maths", PhaseOfEducationValues.Secondary),
            Entry("Attendance guide", "Performance and attendance", "Attendance", PhaseOfEducationValues.Secondary),
            Entry("Literacy guide", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Leadership guide", "Wider school", "Leadership and training", PhaseOfEducationValues.Secondary),
            Entry("Curriculum guide", "Wider school", "Curriculum and teaching", PhaseOfEducationValues.Secondary));

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        var expected = new[] { "Maths", "Attendance", "Literacy", "Leadership and training", "Curriculum and teaching" };

        page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal(expected);

        page.ElementWithTestIdShouldExist("rise-resources-contents")
            .QuerySelectorAll("a").Select(a => a.TrimmedTextContent())
            .Should().Equal(expected);
    }

    [Fact]
    public async Task RiseResources_WhenNoResourcesMatchThePhase_ShowsEmptyState()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupResources(
            Entry("Phonics screening support", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary));

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        page.QuerySelectorAll("[data-testid='rise-resource']").Should().BeEmpty();
        page.ElementWithTestIdShouldExist("rise-resources-empty");
    }
}
