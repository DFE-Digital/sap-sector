using AngleSharp.Html.Dom;
using FluentAssertions;
using SAPSec.Core.Constants;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Primary;

public class ComparisonSimilarityPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task DisposeAsync()
    {
        Fixture.FeatureFlagService.ClearOverrides(FeatureFlags.EnablePrimarySchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task Similarity_NonExistentCurrentSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("999999").Comparison("100002").Similarity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Similarity_NonExistentComparatorSchoolUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("999999").Similarity, HttpStatusCode.NotFound);
    }

    [Fact(Skip = "Not implemented yet")]
    public async Task Similarity_WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", []))
            .SetupValues(Build.PrimaryValues("100001", "100002"));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Similarity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Similarity_WhenSimilarSchoolValuesDoNotExistForCurrentSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues("100002"));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Similarity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Similarity_WhenSimilarSchoolValuesDoNotExistForComparatorSchool_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues("100001"));

        await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Similarity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Similarity_DisplaysCharacteristicsTable_WithCorrectHeadersAndValues()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        Fixture.SimilarSchoolsPrimaryRepository.SetupValues(
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100001",
                Ks1PriorRwmAverage = "100.4",
                PPPerc = "19.44",
                Polar4QuintilePupils = "1.4",
                PStability = "90.5",
                IdaciPupils = "0.1305",
                PercentSchSupport = "10.5",
                NumberOfPupils = "300.5",
                PercentageStatementOrEhp = "2.5",
                PercentEAL = "20.5"
            },
            new SimilarSchoolsPrimaryValuesEntry
            {
                URN = "100002",
                Ks1PriorRwmAverage = "102.6",
                PPPerc = "25.44",
                Polar4QuintilePupils = "2.6",
                PStability = "85.5",
                IdaciPupils = "0.1314",
                PercentSchSupport = "15.5",
                NumberOfPupils = "320.5",
                PercentageStatementOrEhp = "3.5",
                PercentEAL = "30.5"
            });

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Similarity);

        var table = page.QuerySelector("table.govuk-table") as IHtmlTableElement;
        table.Should().NotBeNull();

        table!.ShouldHaveRows(
            ["Characteristic", "Test School 1", "Test School 2"],
            ["Combined average KS1 reading, writing and maths prior attainment", "100", "103"],
            ["Total number of pupils", "301", "321"],
            ["Pupil stability rate", "90.5%", "85.5%"],
            ["Eligibility for pupil premium", "19.4%", "25.4%"],
            ["Average IDACI score", "0.131", "0.131"],
            ["Average POLAR4 quintile", "Quintile 1", "Quintile 3"],
            ["Percentage of pupils with an EHC plan", "2.5%", "3.5%"],
            ["Percentage of pupils with SEN support", "10.5%", "15.5%"],
            ["Percentage of pupils with EAL", "20.5%", "30.5%"]);
    }

    [Fact]
    public async Task Similarity_LinksToWhatIsASimilarSchoolPage()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary().InLA("002")));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues("100001", "100002"));

        var page = await Fixture.RequestPageAsync(
            Routes.PrimarySchool("100001").Comparison("100002").Similarity);

        var link = page.ElementWithTestIdShouldExist<IHtmlAnchorElement>("what-is-a-similar-school-link");

        link.PathName.Should().Be(Routes.PrimarySchool("100001").WhatIsASimilarSchool);
    }
}
