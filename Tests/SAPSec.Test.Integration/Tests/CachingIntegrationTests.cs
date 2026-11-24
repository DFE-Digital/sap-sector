using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.AllThrough;

public class CachingIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Urn = "100001";

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School 1", x => x.Open().Primary()));

        Fixture.Cache.GetAllKeys("UseCase::").Should().BeEmpty();

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        return base.DisposeAsync();
    }

    [Fact]
    public async Task AfterInitialRequest_CacheShouldBePopulated()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.Cache.GetAllKeys("UseCase::").Should().HaveCount(3);
        Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(r => r.SchoolDetails.ShouldMatch(Urn, "Test School 1"));
        Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .Should().SatisfyRespectively(r => r.School.ShouldMatch(Urn, "Test School 1"));
        Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .Should().SatisfyRespectively(r => r.CurrentSchool.ShouldMatch(Urn, "Test School 1"));
    }

    [Fact]
    public async Task AfterSubsequentIdenticalRequest_CacheShouldHaveSameValues()
    {
        await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);

        var schoolDetails = Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .First();
        var performance = Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .First();
        var similarSchools = Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .First();

        await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);

        Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .First().Should().BeEquivalentTo(schoolDetails);
        Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .First().Should().BeEquivalentTo(performance);
        Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .First().Should().BeEquivalentTo(similarSchools);
    }

    [Fact]
    public async Task SubsequentIdenticalRequest_ShouldNotHitDatabase()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "MODIFIED SCHOOL", x => x.Open().Primary()));
        Fixture.EstablishmentRepository.VerifyEstablishment(Urn, e =>
            e.EstablishmentName.Should().Be("MODIFIED SCHOOL"));

        var newPage = await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);
        newPage.SchoolName().Should().Be("Test School 1");
    }

    [Fact]
    public async Task SubsequentIdenticalRequest_ShouldHitDatabaseAgainWhenCacheCleared()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "MODIFIED SCHOOL", x => x.Open().Primary()));
        Fixture.EstablishmentRepository.VerifyEstablishment(Urn, e =>
            e.EstablishmentName.Should().Be("MODIFIED SCHOOL"));

        await Fixture.Cache.Clear();

        var newPage = await Fixture.RequestPageAsync(Routes.PrimarySchool(Urn).KS2);
        newPage.SchoolName().Should().Be("MODIFIED SCHOOL");
    }
}

file static class TestExtensions
{
    public static void ShouldMatch(this SchoolDetails schoolDetails, string urn, string name)
    {
        schoolDetails.Urn.Should().Be(urn);
        schoolDetails.Name.Should().Be(name);
    }
    public static void ShouldMatch(this SchoolInfo schoolInfo, string urn, string name)
    {
        schoolInfo.Urn.Should().Be(urn);
        schoolInfo.Name.Should().Be(name);
    }

    public static string SchoolName(this IDocument document)
    {
        var schoolNameElement = document.QuerySelector("span:has(+ h1.govuk-heading-xl)");
        schoolNameElement.Should().NotBeNull();
        return schoolNameElement.TrimmedTextContent();
    }
}