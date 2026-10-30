using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

/// <summary>
/// The "Comparing to" pages of every phase: school users get no breadcrumbs and a back link that names the
/// similar-schools list; everyone else keeps the breadcrumbs and "Back". The Primary and All-through pages
/// use different layouts from Secondary, which <see cref="SchoolUserAccessIntegrationTests"/> covers.
/// </summary>
public class SchoolUserComparisonLayoutIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string OwnUrn = "100001";
    private const string PrimaryComparatorUrn = "100002";
    private const string SecondaryComparatorUrn = "200002";

    public override Task DisposeAsync()
    {
        Fixture.TestUser.Reset();
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnablePrimarySchools);
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableFindASchool);

        return base.DisposeAsync();
    }

    public static TheoryData<string> ComparisonPhases() => ["Primary", "AllThroughPrimary", "AllThroughSecondary"];

    [Theory]
    [MemberData(nameof(ComparisonPhases))]
    public async Task ComparisonPage_AsSchoolUser_HasNoBreadcrumbsAndBackLinkToSimilarSchools(string phase)
    {
        var (path, similarSchoolsPath) = SetUp(phase);
        Fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
        Fixture.TestUser.Urn = OwnUrn;

        var content = await GetAsync(path);

        content.Should().NotContain("govuk-breadcrumbs");
        BackLinkText(content).Should().Be("View similar schools");
        BackLinkHref(content).Should().Be(similarSchoolsPath);
    }

    [Theory]
    [MemberData(nameof(ComparisonPhases))]
    public async Task ComparisonPage_AsNonSchoolUser_KeepsBreadcrumbsAndBackLabel(string phase)
    {
        var (path, _) = SetUp(phase);

        var content = await GetAsync(path);

        content.Should().Contain("govuk-breadcrumbs");
        BackLinkText(content).Should().Be("Back");
    }

    [Theory]
    [MemberData(nameof(ComparisonPhases))]
    public async Task ComparisonPage_WhenFindASchoolIsEnabled_SchoolUserKeepsBreadcrumbsAndBackLabel(string phase)
    {
        var (path, _) = SetUp(phase);
        Fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
        Fixture.TestUser.Urn = OwnUrn;
        Fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var content = await GetAsync(path);

        content.Should().Contain("govuk-breadcrumbs");
        BackLinkText(content).Should().Be("Back");
    }

    private (string ComparisonPath, string SimilarSchoolsPath) SetUp(string phase)
    {
        Fixture.FeatureFlagService.Override(Flags.EnablePrimarySchools, true);
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);

        switch (phase)
        {
            case "Primary":
                Fixture.EstablishmentRepository.SetupEstablishments(
                    Build.Establishment(OwnUrn, "Current School", x => x.Open().Primary()),
                    Build.Establishment(PrimaryComparatorUrn, "Comparator School", x => x.Open().Primary()));
                SetUpPrimarySimilarSchools();
                return (
                    Routes.PrimarySchool(OwnUrn).Comparison(PrimaryComparatorUrn).Similarity,
                    Routes.PrimarySchool(OwnUrn).ViewSimilarSchools);

            case "AllThroughPrimary":
                Fixture.EstablishmentRepository.SetupEstablishments(
                    Build.Establishment(OwnUrn, "Current School", x => x.Open().AllThrough()),
                    Build.Establishment(PrimaryComparatorUrn, "Comparator School", x => x.Open().Primary()));
                SetUpPrimarySimilarSchools();
                return (
                    Routes.AllThroughSchool(OwnUrn).PrimaryComparison(PrimaryComparatorUrn).Similarity,
                    Routes.AllThroughSchool(OwnUrn).ViewSimilarSchools);

            default:
                Fixture.EstablishmentRepository.SetupEstablishments(
                    Build.Establishment(OwnUrn, "Current School", x => x.Open().AllThrough()),
                    Build.Establishment(SecondaryComparatorUrn, "Comparator School", x => x.Open().Secondary()));
                Fixture.SimilarSchoolsSecondaryRepository
                    .SetupGroups(Build.SecondaryGroup(OwnUrn, [SecondaryComparatorUrn]))
                    .SetupValues(Build.SecondaryValues([OwnUrn, SecondaryComparatorUrn]));
                return (
                    Routes.AllThroughSchool(OwnUrn).SecondaryComparison(SecondaryComparatorUrn).Similarity,
                    Routes.AllThroughSchool(OwnUrn).ViewSimilarSchools + "?phase=secondary");
        }
    }

    private void SetUpPrimarySimilarSchools() =>
        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup(OwnUrn, [PrimaryComparatorUrn]))
            .SetupValues(Build.PrimaryValues([OwnUrn, PrimaryComparatorUrn]));

    private async Task<string> GetAsync(string path)
    {
        var response = await Fixture.Client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private static string BackLinkText(string content) =>
        Regex.Match(content, @"<a[^>]*govuk-back-link[^>]*>\s*(.*?)\s*</a>", RegexOptions.Singleline).Groups[1].Value.Trim();

    private static string BackLinkHref(string content) =>
        Regex.Match(content, @"<a[^>]*govuk-back-link[^>]*href=""([^""]*)""").Groups[1].Value;
}
