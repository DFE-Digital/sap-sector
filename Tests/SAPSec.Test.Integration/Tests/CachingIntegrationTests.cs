using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

public class CachingIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Open().Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Open().Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Open().Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Open().AllThrough()),
            Build.Establishment("100006", "Test School 6", x => x.Open().AllThrough()));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups([
                ..Build.PrimaryGroup("100001", ["100002"]),
                ..Build.PrimaryGroup("100005", ["100006"])])
            .SetupValues(
                Build.PrimaryValues("100001"),
                Build.PrimaryValues("100002"),
                Build.PrimaryValues("100005"),
                Build.PrimaryValues("100006"));

        Fixture.SimilarSchoolsSecondaryRepository
            .SetupGroups([
                ..Build.SecondaryGroup("100003", ["100004"]),
                ..Build.SecondaryGroup("100005", ["100006"])])
            .SetupValues(
                Build.SecondaryValues("100003"),
                Build.SecondaryValues("100004"),
                Build.SecondaryValues("100005"),
                Build.SecondaryValues("100006"));
            
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
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.Cache.GetAllKeys("UseCase::").Should().HaveCount(3);
        Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(r => r.SchoolDetails.ShouldMatch("100001", "Test School 1"));
        Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .Should().SatisfyRespectively(r => r.School.ShouldMatch("100001", "Test School 1"));
        Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .Should().SatisfyRespectively(r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"));
    }

    [Fact]
    public async Task AfterSubsequentIdenticalRequest_CacheShouldHaveSameValues()
    {
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

        var schoolDetails = Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .First();
        var performance = Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .First();
        var similarSchools = Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .First();

        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);

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
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "MODIFIED SCHOOL", x => x.Open().Primary()));
        Fixture.EstablishmentRepository.VerifyEstablishment("100001", e =>
            e.EstablishmentName.Should().Be("MODIFIED SCHOOL"));

        var newPage = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        newPage.SchoolName().Should().Be("Test School 1");
    }

    [Fact]
    public async Task SubsequentIdenticalRequest_ShouldHitDatabaseAgainWhenCacheCleared()
    {
        var page = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        page.SchoolName().Should().Be("Test School 1");

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "MODIFIED SCHOOL", x => x.Open().Primary()));
        Fixture.EstablishmentRepository.VerifyEstablishment("100001", e =>
            e.EstablishmentName.Should().Be("MODIFIED SCHOOL"));

        await Fixture.Cache.Clear();

        var newPage = await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        newPage.SchoolName().Should().Be("MODIFIED SCHOOL");
    }

    [Fact]
    public async Task AfterMultipleRequests_CacheShouldBePopulatedWithCorrectData()
    {
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100002").KS2);

        Fixture.Cache.GetAllKeys("UseCase::").Should().HaveCount(6);
        Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(
                r => r.SchoolDetails.ShouldMatch("100001", "Test School 1"),
                r => r.SchoolDetails.ShouldMatch("100002", "Test School 2"));
        Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100001", "Test School 1"),
                r => r.School.ShouldMatch("100002", "Test School 2"));
        Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100002", "Test School 2"));
    }

    [Fact]
    public async Task AfterMultipleRequestsCallingMultipleUseCases_CacheShouldBePopulatedWithCorrectData()
    {
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Overview);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").ViewSimilarSchools);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").KS2);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Attendance);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").Similarity);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").KS2);
        await Fixture.RequestPageAsync(Routes.PrimarySchool("100001").Comparison("100002").Attendance);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Overview);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").ViewSimilarSchools);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").KS4CoreSubjects);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").KS4HeadlineMeasures);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Attendance);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Comparison("100004").Similarity);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Comparison("100004").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Comparison("100004").KS4HeadlineMeasures);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Comparison("100004").KS4CoreSubjects);
        await Fixture.RequestPageAsync(Routes.SecondarySchool("100003").Comparison("100004").Attendance);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").Overview);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").ViewSimilarSchools);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").KS2);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").KS4CoreSubjects);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").KS4HeadlineMeasures);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").Attendance);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").PrimaryComparison("100006").Similarity);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").PrimaryComparison("100006").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").PrimaryComparison("100006").KS2);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").PrimaryComparison("100006").Attendance);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SecondaryComparison("100006").Similarity);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SecondaryComparison("100006").SchoolDetails);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SecondaryComparison("100006").KS4HeadlineMeasures);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SecondaryComparison("100006").KS4CoreSubjects);
        await Fixture.RequestPageAsync(Routes.AllThroughSchool("100005").SecondaryComparison("100006").Attendance);

        Fixture.Cache.GetAllKeys("UseCase::").Should().Satisfy(
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolInfoUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolInfoUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolInfoUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::FindPrimarySimilarSchoolsUseCase::"),
            k => k.StartsWith("UseCase::FindPrimarySimilarSchoolsUseCase::"),
            k => k.StartsWith("UseCase::FindPrimarySimilarSchoolsUseCase::"),
            k => k.StartsWith("UseCase::FindSecondarySimilarSchoolsUseCase::"),
            k => k.StartsWith("UseCase::FindSecondarySimilarSchoolsUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs4HeadlineMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs4HeadlineMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs4CoreSubjectsMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSchoolKs4CoreSubjectsMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonSimilarityCharacteristicsUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonSimilarityCharacteristicsUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetPrimaryComparisonAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs2PerformanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs2PerformanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonSimilarityCharacteristicsUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonSimilarityCharacteristicsUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonSchoolDetailsUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetSecondaryComparisonAttendanceMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs4CoreSubjectsMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs4CoreSubjectsMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs4HeadlineMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetComparisonKs4HeadlineMeasuresUseCase::"),
            k => k.StartsWith("UseCase::GetAllThroughSimilarSchoolPhasesUseCase::"));

        Fixture.Cache.GetAllValues<GetSchoolDetailsResponse>("UseCase::GetSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(
                r => r.SchoolDetails.ShouldMatch("100001", "Test School 1"),
                r => r.SchoolDetails.ShouldMatch("100002", "Test School 2"),
                r => r.SchoolDetails.ShouldMatch("100003", "Test School 3"),
                r => r.SchoolDetails.ShouldMatch("100004", "Test School 4"),
                r => r.SchoolDetails.ShouldMatch("100005", "Test School 5"),
                r => r.SchoolDetails.ShouldMatch("100006", "Test School 6"));
        Fixture.Cache.GetAllValues<GetSchoolInfoResponse>("UseCase::GetSchoolInfoUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100001", "Test School 1"),
                r => r.School.ShouldMatch("100003", "Test School 3"),
                r => r.School.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetSchoolAttendanceMeasuresResponse>("UseCase::GetSchoolAttendanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100001", "Test School 1"),
                r => r.School.ShouldMatch("100003", "Test School 3"),
                r => r.School.ShouldMatch("100005", "Test School 5"),
                r => r.School.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<FindPrimarySimilarSchoolsResponse>("UseCase::FindPrimarySimilarSchoolsUseCase::")
            .Should().SatisfyRespectively(
                //TODO: why is this in twice?
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<FindSecondarySimilarSchoolsResponse>("UseCase::FindSecondarySimilarSchoolsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"));
        Fixture.Cache.GetAllValues<GetSchoolKs2PerformanceMeasuresResponse>("UseCase::GetSchoolKs2PerformanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100001", "Test School 1"),
                r => r.School.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetSchoolKs4HeadlineMeasuresResponse>("UseCase::GetSchoolKs4HeadlineMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100003", "Test School 3"),
                r => r.School.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetSchoolKs4CoreSubjectsMeasuresResponse>("UseCase::GetSchoolKs4CoreSubjectsMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.School.ShouldMatch("100003", "Test School 3"),
                r => r.School.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetPrimaryComparisonSimilarityCharacteristicsResponse>("UseCase::GetPrimaryComparisonSimilarityCharacteristicsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonSchoolDetailsResponse>("UseCase::GetPrimaryComparisonSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonAttendanceMeasuresResponse>("UseCase::GetPrimaryComparisonAttendanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonKs2PerformanceMeasuresResponse>("UseCase::GetComparisonKs2PerformanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100001", "Test School 1"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetSecondaryComparisonSimilarityCharacteristicsResponse>("UseCase::GetSecondaryComparisonSimilarityCharacteristicsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonSchoolDetailsResponse>("UseCase::GetSecondaryComparisonSchoolDetailsUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonAttendanceMeasuresResponse>("UseCase::GetSecondaryComparisonAttendanceMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonKs4HeadlineMeasuresResponse>("UseCase::GetComparisonKs4HeadlineMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetComparisonKs4CoreSubjectsMeasuresResponse>("UseCase::GetComparisonKs4CoreSubjectsMeasuresUseCase::")
            .Should().SatisfyRespectively(
                r => r.CurrentSchool.ShouldMatch("100003", "Test School 3"),
                r => r.CurrentSchool.ShouldMatch("100005", "Test School 5"));
        Fixture.Cache.GetAllValues<GetAllThroughSimilarSchoolPhasesResponse>("UseCase::GetAllThroughSimilarSchoolPhasesUseCase::")
            .Should().SatisfyRespectively(
                r => r.Urn.Should().Be("100005"));
    }
}

file static class TestExtensions
{
    public static void ShouldMatch(this SchoolDetails school, string urn, string name)
    {
        school.Urn.Should().Be(urn);
        school.Name.Should().Be(name);
    }

    public static void ShouldMatch(this SchoolInfo school, string urn, string name)
    {
        school.Urn.Should().Be(urn);
        school.Name.Should().Be(name);
    }
    
    public static void ShouldMatch(this SchoolWithCoordinates school, string urn, string name)
    {
        school.School.Urn.Should().Be(urn);
        school.School.Name.Should().Be(name);
    }

    public static string SchoolName(this IDocument document)
    {
        var schoolNameElement = document.QuerySelector("span:has(+ h1.govuk-heading-xl)");
        schoolNameElement.Should().NotBeNull();
        return schoolNameElement.TrimmedTextContent();
    }
}