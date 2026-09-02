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
    public async Task RiseResources_GroupsResourcesByCategoryThenSubCategory_ForAPrimarySchool()
    {
        Fixture.FeatureFlagService.Override(FeatureFlags.EnableRiseResources, true);
        Fixture.RiseResourcesRepository.SetupResources(
            Entry("Choosing a validated phonics teaching programme", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary),
            Entry("Improving Literacy in KS2", "Performance and attendance", "Literacy", PhaseOfEducationValues.Primary, "All through"),
            Entry("Teaching maths fluency", "Performance and attendance", "Maths", PhaseOfEducationValues.Primary),
            Entry("Raising GCSE attainment", "Performance and attendance", "Maths", PhaseOfEducationValues.Secondary));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").RiseResources, HttpStatusCode.OK);

        var headings = page.QuerySelectorAll(
                "[data-testid='rise-resources-category'], [data-testid='rise-resources-subcategory']")
            .Select(el => el.TrimmedTextContent());
        headings.Should().Equal("Performance and attendance", "Literacy", "Maths");

        var literacyList = page.QuerySelectorAll("[data-testid='rise-resources-subcategory']")
            .First(heading => heading.TrimmedTextContent() == "Literacy")
            .NextElementSibling;
        literacyList!.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().Equal(
                "Choosing a validated phonics teaching programme (opens in new tab)",
                "Improving Literacy in KS2 (opens in new tab)");

        // Secondary-only resource is excluded for a primary school.
        page.QuerySelectorAll("[data-testid='rise-resource-title']")
            .Select(el => el.TrimmedTextContent())
            .Should().NotContain(title => title.Contains("Raising GCSE attainment"));
    }
}
