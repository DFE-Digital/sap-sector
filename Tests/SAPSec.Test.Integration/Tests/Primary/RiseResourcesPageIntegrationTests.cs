using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class RiseResourcesPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")));

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
    public async Task RiseResources_WhenEnableRiseResourcesFeatureFlagEnabled_ReturnsOk()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").RiseResources, HttpStatusCode.OK);

        var heading = page.QuerySelector("h1.govuk-heading-xl");
        heading.Should().NotBeNull();
        heading!.TextContent.Trim().Should().Be(PageTitles.RiseResources);
    }

    [Fact]
    public async Task RiseResources_WhenEnableRiseResourcesFeatureFlagDisabled_ReturnsNotFound()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, false);

        var response = await Fixture.Client.GetAsync(Routes.PrimarySchool("100001").RiseResources);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RiseResources_WithNonExistentUrn_ReturnsNotFound()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);

        var response = await Fixture.Client.GetAsync(Routes.PrimarySchool("999999").RiseResources);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RiseResources_GroupsByCategoryThenSubCategory_ForAPrimarySchool()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupCategories(
            Category("Curriculum", "Resources covering curriculum and teaching."));
        Fixture.RiseResourcesRepository.SetupResources(
            // File order puts "Improving Literacy in KS2" before "Choosing a validated phonics teaching programme".
            Entry("Improving Literacy in KS2", "Curriculum", "Literacy", PhaseOfEducationValues.Primary, "All through"),
            Entry("Choosing a validated phonics teaching programme", "Curriculum", "Literacy", PhaseOfEducationValues.Primary),
            Entry("Teaching maths fluency", "Curriculum", "Maths", PhaseOfEducationValues.Primary),
            Entry("Raising GCSE attainment", "Curriculum", "Maths", PhaseOfEducationValues.Secondary));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").RiseResources, HttpStatusCode.OK);

        page.ElementWithTestIdShouldExist("rise-resources-contents")
            .QuerySelectorAll("a").Select(a => a.TrimmedTextContent())
            .Should().Equal("Curriculum");
        page.ElementWithTestIdShouldExist("rise-resources-category-description")
            .TrimmedTextContent().Should().Be("Resources covering curriculum and teaching.");

        page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal("Literacy", "Maths");

        // Alphabetical within the sub-category; "All through" resource is included for a primary school.
        var literacyList = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .First(heading => heading.TrimmedTextContent() == "Literacy")
            .NextElementSibling;
        literacyList!.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal(
                "Choosing a validated phonics teaching programme",
                "Improving Literacy in KS2");

        // Secondary-only resource is excluded for a primary school.
        page.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().NotContain(title => title.Contains("Raising GCSE attainment"));

        // Resource link href is the configured URL (from JSON, not hard-coded).
        literacyList.QuerySelector("a")!.GetAttribute("href")
            .Should().Be("https://example.gov.uk/choosing-a-validated-phonics-teaching-programme");
    }
}
