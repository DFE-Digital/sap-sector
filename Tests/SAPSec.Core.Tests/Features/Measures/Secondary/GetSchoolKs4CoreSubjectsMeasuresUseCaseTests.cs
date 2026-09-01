using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Secondary;

namespace SAPSec.Core.Tests.Features.Measures.Secondary;

public class GetSchoolKs4CoreSubjectsMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo;
    private readonly InMemoryKs4PerformanceRepository _performanceRepo;
    private readonly GetSchoolKs4CoreSubjectsMeasuresUseCase _sut;

    public GetSchoolKs4CoreSubjectsMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _similarSchoolsRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _sut = new GetSchoolKs4CoreSubjectsMeasuresUseCase(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo);
    }

    [Fact]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(Request("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task School_ShouldContainCurrentSchoolInfo()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .Secondary()
                .WithAddress("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG")
                .InLA("001", "Test LA")));

        var response = await _sut.Execute(Request("100001"));

        response.School.Urn.Should().Be("100001");
        response.School.Name.Should().Be("Test School");
        response.School.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.School.LocalAuthority.Should().Be(
            new LocalAuthority("001", "Test LA"));
    }

    [Fact]
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "18", prev: "75", prev2: "80")
                .WithEngLit49(current: "18", prev: "75", prev2: "80")
                .WithMaths49(current: "18", prev: "75", prev2: "80")
                .WithCombSci49(current: "18", prev: "75", prev2: "80")
                .WithBio49(current: "18", prev: "75", prev2: "80")
                .WithChem49(current: "18", prev: "75", prev2: "80")
                .WithPhysics49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "20", prev: "70", prev2: "50")
                .WithEngLit49(current: "20", prev: "70", prev2: "50")
                .WithMaths49(current: "20", prev: "70", prev2: "50")
                .WithCombSci49(current: "20", prev: "70", prev2: "50")
                .WithBio49(current: "20", prev: "70", prev2: "50")
                .WithChem49(current: "20", prev: "70", prev2: "50")
                .WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLang49(current: "21", prev: "69", prev2: "51")
                .WithEngLit49(current: "21", prev: "69", prev2: "51")
                .WithMaths49(current: "21", prev: "69", prev2: "51")
                .WithCombSci49(current: "21", prev: "69", prev2: "51")
                .WithBio49(current: "21", prev: "69", prev2: "51")
                .WithChem49(current: "21", prev: "69", prev2: "51")
                .WithPhysics49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithEngLang49(current: "22", prev: "68", prev2: "49")
                .WithEngLit49(current: "22", prev: "68", prev2: "49")
                .WithMaths49(current: "22", prev: "68", prev2: "49")
                .WithCombSci49(current: "22", prev: "68", prev2: "49")
                .WithBio49(current: "22", prev: "68", prev2: "49")
                .WithChem49(current: "22", prev: "68", prev2: "49")
                .WithPhysics49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x
                .WithEngLang49(current: "19", prev: "61", prev2: "67")
                .WithEngLit49(current: "19", prev: "61", prev2: "67")
                .WithMaths49(current: "19", prev: "61", prev2: "67")
                .WithCombSci49(current: "19", prev: "61", prev2: "67")
                .WithBio49(current: "19", prev: "61", prev2: "67")
                .WithChem49(current: "19", prev: "61", prev2: "67")
                .WithPhysics49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Ks4EnglishLanguage.Filters.Grade.Key] = Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove,
            [Ks4EnglishLiterature.Filters.Grade.Key] = Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove,
            [Ks4Maths.Filters.Grade.Key] = Ks4Maths.Filters.Grade.Values.Grade4AndAbove,
            [Ks4CombinedScience.Filters.Grade.Key] = Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove,
            [Ks4Biology.Filters.Grade.Key] = Ks4Biology.Filters.Grade.Values.Grade4AndAbove,
            [Ks4Chemistry.Filters.Grade.Key] = Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove,
            [Ks4Physics.Filters.Grade.Key] = Ks4Physics.Filters.Grade.Values.Grade4AndAbove,
            ["yyy"] = "2",
        }));

        response.EnglishLanguage.Series.Should().NotBeEmpty();
        response.EnglishLiterature.Series.Should().NotBeEmpty();
        response.Maths.Series.Should().NotBeEmpty();
        response.CombinedScience.Series.Should().NotBeEmpty();
        response.Biology.Series.Should().NotBeEmpty();
        response.Chemistry.Series.Should().NotBeEmpty();
        response.Physics.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EnglishLanguage_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.EnglishLanguage.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLanguage_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLanguage_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLang49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLang49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLanguage_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task EnglishLanguage_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLang49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task EnglishLanguage_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishLanguage_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLang49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task EnglishLanguage_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLang49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLanguage_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLang49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngLang49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task EnglishLanguage_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLanguage.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLanguage.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishLanguage_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "72", prev: "71", prev2: "70")
                .WithEngLang59(current: "62", prev: "61", prev2: "60")
                .WithEngLang79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "72", prev: "71", prev2: "70")
                .WithEngLang59(current: "60", prev: "59", prev2: "58")
                .WithEngLang79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLang49(current: "70", prev: "69", prev2: "68")
                .WithEngLang59(current: "62", prev: "61", prev2: "60")
                .WithEngLang79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngLang49(current: "73", prev: "72", prev2: "71")
                .WithEngLang59(current: "63", prev: "62", prev2: "61")
                .WithEngLang79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLang49(current: "74", prev: "73", prev2: "72")
                .WithEngLang59(current: "64", prev: "63", prev2: "62")
                .WithEngLang79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLanguage.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLanguage.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task EnglishLanguage_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "30", prev: "", prev2: "")
                .WithEngLang59(current: "96", prev: "", prev2: "")
                .WithEngLang79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "20", prev: "", prev2: "")
                .WithEngLang59(current: "97", prev: "", prev2: "")
                .WithEngLang79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLang49(current: "10", prev: "", prev2: "")
                .WithEngLang59(current: "98", prev: "", prev2: "")
                .WithEngLang79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithEngLang49(current: "0", prev: "", prev2: "")
                .WithEngLang59(current: "99", prev: "", prev2: "")
                .WithEngLang79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLanguage.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.EnglishLanguage.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task EnglishLiterature_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.EnglishLiterature.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLiterature_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLiterature_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLit49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLit49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLiterature_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task EnglishLiterature_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLit49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task EnglishLiterature_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishLiterature_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLit49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task EnglishLiterature_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngLit49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishLiterature_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLit49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngLit49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngLit49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task EnglishLiterature_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLiterature.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLiterature.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishLiterature_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "72", prev: "71", prev2: "70")
                .WithEngLit59(current: "62", prev: "61", prev2: "60")
                .WithEngLit79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "72", prev: "71", prev2: "70")
                .WithEngLit59(current: "60", prev: "59", prev2: "58")
                .WithEngLit79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLit49(current: "70", prev: "69", prev2: "68")
                .WithEngLit59(current: "62", prev: "61", prev2: "60")
                .WithEngLit79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngLit49(current: "73", prev: "72", prev2: "71")
                .WithEngLit59(current: "63", prev: "62", prev2: "61")
                .WithEngLit79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLit49(current: "74", prev: "73", prev2: "72")
                .WithEngLit59(current: "64", prev: "63", prev2: "62")
                .WithEngLit79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLiterature.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLiterature.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task EnglishLiterature_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "30", prev: "", prev2: "")
                .WithEngLit59(current: "96", prev: "", prev2: "")
                .WithEngLit79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "20", prev: "", prev2: "")
                .WithEngLit59(current: "97", prev: "", prev2: "")
                .WithEngLit79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngLit49(current: "10", prev: "", prev2: "")
                .WithEngLit59(current: "98", prev: "", prev2: "")
                .WithEngLit79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithEngLit49(current: "0", prev: "", prev2: "")
                .WithEngLit59(current: "99", prev: "", prev2: "")
                .WithEngLit79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishLiterature.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.EnglishLiterature.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task Maths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Maths.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Maths_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Maths_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Maths_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Maths_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithMaths49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Maths_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Maths_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithMaths49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Maths_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithMaths49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Maths_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Maths_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Maths_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Maths_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Maths_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Maths_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithMaths49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithMaths49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4Maths.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Maths_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Maths.Filters.Grade.Key] = subject
        }));

        var series = response.Maths.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Maths.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Maths_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "72", prev: "71", prev2: "70")
                .WithMaths59(current: "62", prev: "61", prev2: "60")
                .WithMaths79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "72", prev: "71", prev2: "70")
                .WithMaths59(current: "60", prev: "59", prev2: "58")
                .WithMaths79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithMaths49(current: "70", prev: "69", prev2: "68")
                .WithMaths59(current: "62", prev: "61", prev2: "60")
                .WithMaths79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithMaths49(current: "73", prev: "72", prev2: "71")
                .WithMaths59(current: "63", prev: "62", prev2: "61")
                .WithMaths79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithMaths49(current: "74", prev: "73", prev2: "72")
                .WithMaths59(current: "64", prev: "63", prev2: "62")
                .WithMaths79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Maths.Filters.Grade.Key] = subject
        }));

        var series = response.Maths.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4Maths.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task Maths_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "30", prev: "", prev2: "")
                .WithMaths59(current: "96", prev: "", prev2: "")
                .WithMaths79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "20", prev: "", prev2: "")
                .WithMaths59(current: "97", prev: "", prev2: "")
                .WithMaths79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithMaths49(current: "10", prev: "", prev2: "")
                .WithMaths59(current: "98", prev: "", prev2: "")
                .WithMaths79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithMaths49(current: "0", prev: "", prev2: "")
                .WithMaths59(current: "99", prev: "", prev2: "")
                .WithMaths79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Maths.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.Maths.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task CombinedScience_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.CombinedScience.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task CombinedScience_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task CombinedScience_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithCombSci49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithCombSci49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task CombinedScience_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task CombinedScience_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithCombSci49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task CombinedScience_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task CombinedScience_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithCombSci49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task CombinedScience_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithCombSci49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task CombinedScience_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithCombSci49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithCombSci49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove)]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove)]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove)]
    [Theory]
    public async Task CombinedScience_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4CombinedScience.Filters.Grade.Key] = subject
        }));

        var series = response.CombinedScience.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task CombinedScience_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "72", prev: "71", prev2: "70")
                .WithCombSci59(current: "62", prev: "61", prev2: "60")
                .WithCombSci79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "72", prev: "71", prev2: "70")
                .WithCombSci59(current: "60", prev: "59", prev2: "58")
                .WithCombSci79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithCombSci49(current: "70", prev: "69", prev2: "68")
                .WithCombSci59(current: "62", prev: "61", prev2: "60")
                .WithCombSci79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithCombSci49(current: "73", prev: "72", prev2: "71")
                .WithCombSci59(current: "63", prev: "62", prev2: "61")
                .WithCombSci79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithCombSci49(current: "74", prev: "73", prev2: "72")
                .WithCombSci59(current: "64", prev: "63", prev2: "62")
                .WithCombSci79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4CombinedScience.Filters.Grade.Key] = subject
        }));

        var series = response.CombinedScience.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task CombinedScience_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "30", prev: "", prev2: "")
                .WithCombSci59(current: "96", prev: "", prev2: "")
                .WithCombSci79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "20", prev: "", prev2: "")
                .WithCombSci59(current: "97", prev: "", prev2: "")
                .WithCombSci79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithCombSci49(current: "10", prev: "", prev2: "")
                .WithCombSci59(current: "98", prev: "", prev2: "")
                .WithCombSci79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithCombSci49(current: "0", prev: "", prev2: "")
                .WithCombSci59(current: "99", prev: "", prev2: "")
                .WithCombSci79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4CombinedScience.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.CombinedScience.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task Biology_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Biology.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Biology_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Biology_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithBio49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithBio49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Biology_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Biology_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithBio49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Biology_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Biology_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithBio49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Biology_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithBio49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Biology_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Biology_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Biology_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Biology_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Biology_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Biology_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithBio49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithBio49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithBio49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4Biology.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Biology_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Biology.Filters.Grade.Key] = subject
        }));

        var series = response.Biology.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Biology.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Biology_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "72", prev: "71", prev2: "70")
                .WithBio59(current: "62", prev: "61", prev2: "60")
                .WithBio79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "72", prev: "71", prev2: "70")
                .WithBio59(current: "60", prev: "59", prev2: "58")
                .WithBio79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithBio49(current: "70", prev: "69", prev2: "68")
                .WithBio59(current: "62", prev: "61", prev2: "60")
                .WithBio79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithBio49(current: "73", prev: "72", prev2: "71")
                .WithBio59(current: "63", prev: "62", prev2: "61")
                .WithBio79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithBio49(current: "74", prev: "73", prev2: "72")
                .WithBio59(current: "64", prev: "63", prev2: "62")
                .WithBio79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Biology.Filters.Grade.Key] = subject
        }));

        var series = response.Biology.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4Biology.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task Biology_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "30", prev: "", prev2: "")
                .WithBio59(current: "96", prev: "", prev2: "")
                .WithBio79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "20", prev: "", prev2: "")
                .WithBio59(current: "97", prev: "", prev2: "")
                .WithBio79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithBio49(current: "10", prev: "", prev2: "")
                .WithBio59(current: "98", prev: "", prev2: "")
                .WithBio79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithBio49(current: "0", prev: "", prev2: "")
                .WithBio59(current: "99", prev: "", prev2: "")
                .WithBio79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Biology.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.Biology.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task Chemistry_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Chemistry.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Chemistry_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Chemistry_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithChem49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithChem49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Chemistry_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Chemistry_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithChem49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Chemistry_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Chemistry_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithChem49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Chemistry_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithChem49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Chemistry_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithChem49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithChem49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithChem49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Chemistry_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Chemistry.Filters.Grade.Key] = subject
        }));

        var series = response.Chemistry.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Chemistry_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "72", prev: "71", prev2: "70")
                .WithChem59(current: "62", prev: "61", prev2: "60")
                .WithChem79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "72", prev: "71", prev2: "70")
                .WithChem59(current: "60", prev: "59", prev2: "58")
                .WithChem79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithChem49(current: "70", prev: "69", prev2: "68")
                .WithChem59(current: "62", prev: "61", prev2: "60")
                .WithChem79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithChem49(current: "73", prev: "72", prev2: "71")
                .WithChem59(current: "63", prev: "62", prev2: "61")
                .WithChem79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithChem49(current: "74", prev: "73", prev2: "72")
                .WithChem59(current: "64", prev: "63", prev2: "62")
                .WithChem79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Chemistry.Filters.Grade.Key] = subject
        }));

        var series = response.Chemistry.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task Chemistry_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "30", prev: "", prev2: "")
                .WithChem59(current: "96", prev: "", prev2: "")
                .WithChem79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "20", prev: "", prev2: "")
                .WithChem59(current: "97", prev: "", prev2: "")
                .WithChem79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithChem49(current: "10", prev: "", prev2: "")
                .WithChem59(current: "98", prev: "", prev2: "")
                .WithChem79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithChem49(current: "0", prev: "", prev2: "")
                .WithChem59(current: "99", prev: "", prev2: "")
                .WithChem79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Chemistry.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.Chemistry.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task Physics_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Physics.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Physics_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Physics_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithPhysics49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithPhysics49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Physics_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Physics_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithPhysics49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Physics_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Physics_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithPhysics49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Physics_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithPhysics49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Physics_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Physics_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Physics_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Physics_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Physics_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Physics_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithPhysics49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithPhysics49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithPhysics49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4Physics.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Physics_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Physics.Filters.Grade.Key] = subject
        }));

        var series = response.Physics.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Physics.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Physics_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "72", prev: "71", prev2: "70")
                .WithPhysics59(current: "62", prev: "61", prev2: "60")
                .WithPhysics79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "72", prev: "71", prev2: "70")
                .WithPhysics59(current: "60", prev: "59", prev2: "58")
                .WithPhysics79(current: "50", prev: "49", prev2: "48")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithPhysics49(current: "70", prev: "69", prev2: "68")
                .WithPhysics59(current: "62", prev: "61", prev2: "60")
                .WithPhysics79(current: "52", prev: "51", prev2: "50")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithPhysics49(current: "73", prev: "72", prev2: "71")
                .WithPhysics59(current: "63", prev: "62", prev2: "61")
                .WithPhysics79(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithPhysics49(current: "74", prev: "73", prev2: "72")
                .WithPhysics59(current: "64", prev: "63", prev2: "62")
                .WithPhysics79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Physics.Filters.Grade.Key] = subject
        }));

        var series = response.Physics.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4Physics.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade7AndAbove, new[] { "100004", "100001", "100003" })]
    [Theory]
    public async Task Physics_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "30", prev: "", prev2: "")
                .WithPhysics59(current: "96", prev: "", prev2: "")
                .WithPhysics79(current: "53", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "20", prev: "", prev2: "")
                .WithPhysics59(current: "97", prev: "", prev2: "")
                .WithPhysics79(current: "51", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithPhysics49(current: "10", prev: "", prev2: "")
                .WithPhysics59(current: "98", prev: "", prev2: "")
                .WithPhysics79(current: "52", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithPhysics49(current: "0", prev: "", prev2: "")
                .WithPhysics59(current: "99", prev: "", prev2: "")
                .WithPhysics79(current: "54", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Physics.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.Physics.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    private GetSchoolKs4CoreSubjectsMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}
