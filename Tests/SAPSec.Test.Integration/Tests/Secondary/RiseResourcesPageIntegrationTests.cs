using AngleSharp.Dom;
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

    [Fact]
    public async Task RiseResources_WhenEnableRiseResourcesFeatureFlagEnabled_ReturnsOk()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading!.TextContent.Trim().Should().Be(PageTitles.RiseResources);
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
    public async Task RiseResources_GroupsResourcesByCategoryThenSubCategory_ForTheSchoolPhase()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupResources(
            Entry("Improving Literacy in KS2", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Reading House", "Performance and attendance", "Literacy", PhaseOfEducationValues.Secondary),
            Entry("Teaching maths fluency", "Performance and attendance", "Maths", PhaseOfEducationValues.Secondary),
            Entry("Improving attendance", "Performance and attendance", "Attendance", PhaseOfEducationValues.Secondary, "All through"),
            Entry("Phonics screening support", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary));

        var page = await Fixture.RequestPageAsync(
            Routes.SecondarySchool("100001").RiseResources, HttpStatusCode.OK);

        // Section headings render in content-file order: the category once, then each of its sub-categories.
        var headings = page.QuerySelectorAll(
                "[data-testid='rise-resources-category'], [data-testid='rise-resources-subcategory']")
            .Select(el => el.TrimmedTextContent());
        headings.Should().Equal("Performance and attendance", "Literacy", "Maths", "Attendance");

        // Resources sit in the list directly after their sub-category heading; primary-only resource is excluded.
        var literacyList = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .First(heading => heading.TrimmedTextContent() == "Literacy")
            .NextElementSibling;
        literacyList!.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal(
                "Improving Literacy in KS2 (opens in new tab)",
                "Reading House (opens in new tab)");

        var link = literacyList.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("https://example.gov.uk/improving-literacy-in-ks2");
        link.GetAttribute("target").Should().Be("_blank");
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
