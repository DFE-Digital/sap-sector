using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class OverviewPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private static readonly PageTestCase[] PrimaryPages = [
        new(Routes.PrimarySchool("100001").Overview, "Test School 1", NavigationText: "Overview"),
        new(Routes.PrimarySchool("100001").KS2, "KS2 performance measures", NavigationText: "KS2"),
        new(Routes.PrimarySchool("100001").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.PrimarySchool("100001").ViewSimilarSchools, "View similar schools"),
        new(Routes.PrimarySchool("100001").SchoolDetails, "School details"),
        new(Routes.PrimarySchool("100001").WhatIsASimilarSchool, "What is a similar school?"),
        new(Routes.PrimarySchool("100001").RiseResources, "RISE resources"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Similarity, "How these schools compare", NavigationText: "Similarity"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Ks2, "KS2 performance measures", NavigationText: "KS2"),
        new(Routes.PrimarySchool("100001").Comparison("100002").Attendance, "Attendance measures", NavigationText: "Attendance"),
        new(Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails, "School details", NavigationText: "School details")
    ];

    private static readonly Regex ComparisonPage = new Regex(Routes.PrimarySchool(@"\d{6}").Comparison(@"\d{6}").BasePath, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OverviewPage = new Regex(Routes.PrimarySchool(@"\d{6}").Overview, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Open().Primary().InLA("003")));

        //Fixture.SimilarSchoolsPrimaryRepository.SetupGroups(
        //    Build.PrimaryGroup("100001", ["100002", "100003"]));

        //Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
        //    Build.PrimaryValues(["100001", "100002", "100003"]));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Theory]
    [MemberData(nameof(NonComparisonPages))]
    public async Task OverviewPage_Navigation_DoesNotShowViewSimilarSchoolsWhenNoSimilarSchools(string path, string navigationText)
    {
        var page = await Fixture.RequestPageAsync("/school/primary/100001");

        var navigationItems = page.QuerySelectorAll(".app-side-navigation__item a");

        var hrefs = navigationItems.Cast<IHtmlAnchorElement>().Select(a => a.Href).ToArray();

        hrefs.Any(h => h.IndexOf("ViewSimilarSchools", StringComparison.OrdinalIgnoreCase) >= 0).Should().BeFalse();

        //var navigationAssertions = PrimaryPages
        //    .Where(p => !ComparisonPage.IsMatch(p.Path)
        //                && (p.Path.IndexOf("ViewSimilarSchools", StringComparison.OrdinalIgnoreCase) < 0))
        //    .Select(p => new Action<IElement>(n => n.ShouldLinkTo(p.NavigationText ?? p.Heading, p.Path)))
        //    .ToArray();

        //navigationItems.Should().SatisfyRespectively(navigationAssertions);
    }

    public static TheoryData<string, string> NonComparisonPages()
    {
        var data = new TheoryData<string, string>();
        foreach (var page in PrimaryPages)
        {
            if (!ComparisonPage.IsMatch(page.Path))
            {
                data.Add(page.Path, page.NavigationText ?? page.Heading);
            }
        }

        return data;
    }
    private record PageTestCase(string Path, string Heading, string? NavigationText = null);

}