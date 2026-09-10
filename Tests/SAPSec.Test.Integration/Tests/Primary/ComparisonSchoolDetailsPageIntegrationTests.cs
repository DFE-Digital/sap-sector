using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonSchoolDetailsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task NonExistentCurrentSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("999999").Comparison("100002").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NonExistentComparatorSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("999999").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HeadingAndTitle_ReflectCurrentAndComparatorSchools()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        page.Title.Should().Be("Comparator School - Get school improvement insights - GOV.UK");

        var heading = page.ElementShouldExist("h1.govuk-heading-xl");
        heading.TrimmedTextContent().Should().Be("Comparator School");

        var caption = page.ElementShouldExist(".govuk-caption-xl");
        caption.TrimmedTextContent().Should().Be("Current School");
    }

    [Fact]
    public async Task ContactDetailsSection_ContainsSchoolContactDetails()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x
                .Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x
                .Open().Primary()
                .WithTelephone("01234 567890")
                .WithWebsite("https://similar-school.example.com")));

        Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002"]));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);

        var website = page.ElementWithTestIdShouldExist("website")
            .ChildElementShouldExist<IHtmlAnchorElement>("dd a");
        website.TrimmedTextContent().Should().Be("https://similar-school.example.com (opens in new tab)");
        website.Href.Should().Be("https://similar-school.example.com/");

        var telephone = page.ElementWithTestIdShouldExist("telephone")
            .ChildElementShouldExist("dd");
        telephone.TrimmedTextContent().Should().Be("01234 567890");
    }
}
