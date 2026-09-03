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
            // Resource-file order is Wider school first, but resourceCategories order puts it second.
            Entry("Teaching maths fluency", "Wider school", "Curriculum and teaching", PhaseOfEducationValues.Secondary),
            Entry("Reading House", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Improving Literacy in KS2", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Improving attendance", "Performance and attendance", "Attendance", PhaseOfEducationValues.Secondary, "All through"),
            Entry("Pastoral support", "Pupil characteristics", "SEND", PhaseOfEducationValues.Secondary),
            Entry("Phonics screening support", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary));

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        // Contents links + category headings: resourceCategories order first, then unlisted categories.
        var contents = page.ElementWithTestIdShouldExist("rise-resources-contents");
        contents.QuerySelectorAll("a").Select(a => a.TrimmedTextContent())
            .Should().Equal("Performance and attendance", "Wider school", "Pupil characteristics");
        contents.QuerySelector("a")!.GetAttribute("href").Should().Be("#performance-and-attendance");

        var categories = page.QuerySelectorAll("[data-testid='rise-resources-category']");
        categories.Select(el => el.TrimmedTextContent())
            .Should().Equal("Performance and attendance", "Wider school", "Pupil characteristics");
        categories[0].GetAttribute("id").Should().Be("performance-and-attendance");
        categories[0].GetAttribute("tabindex").Should().Be("-1");

        // Category description shown for listed categories, absent for the unlisted one.
        var descriptions = page.QuerySelectorAll("[data-testid='rise-resources-category-description']")
            .Select(el => el.TrimmedTextContent());
        descriptions.Should().Equal("About performance and attendance.", "About the wider school.");

        // Sub-categories follow content-file order within their category.
        page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("Literacy", "Attendance", "Curriculum and teaching", "SEND");

        // Resources within a sub-category: alphabetical by title; primary-only resource excluded.
        var literacyList = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .First(heading => heading.TrimmedTextContent() == "Literacy")
            .NextElementSibling;
        literacyList!.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("Improving Literacy in KS2", "Reading House");

        // Link href is the configured URL (from JSON, not hard-coded).
        var link = literacyList.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("https://example.gov.uk/improving-literacy-in-ks2");
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
