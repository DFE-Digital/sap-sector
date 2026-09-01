using FluentAssertions;
using SAPSec.Core.Constants;
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
}
