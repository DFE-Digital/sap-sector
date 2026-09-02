using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Secondary;

namespace SAPSec.Core.Tests.Features.Measures.Secondary;

public class GetSchoolKs4HeadlineMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo;
    private readonly InMemoryKs4PerformanceRepository _performanceRepo;
    private readonly InMemoryKs4DestinationsRepository _destinationsRepo;
    private readonly GetSchoolKs4HeadlineMeasuresUseCase _sut;

    public GetSchoolKs4HeadlineMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _similarSchoolsRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _destinationsRepo = new(_establishmentRepo);
        _sut = new GetSchoolKs4HeadlineMeasuresUseCase(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo,
            _destinationsRepo);
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
                .WithAttainment8(current: "18", prev: "75", prev2: "80")
                .WithEngMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithAttainment8(current: "20", prev: "70", prev2: "50")
                .WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithAttainment8(current: "21", prev: "69", prev2: "51")
                .WithEngMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithAttainment8(current: "22", prev: "68", prev2: "49")
                .WithEngMaths49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x
                .WithAttainment8(current: "19", prev: "61", prev2: "67")
                .WithEngMaths49(current: "19", prev: "61", prev2: "67")));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithEducation(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithEducation(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100003", x => x
                .WithEducation(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100004", x => x
                .WithEducation(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Destinations.Establishment("100005", x => x
                .WithEducation(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Ks4EnglishMaths.Filters.Grade.Key] = Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove,
            [Ks4Destinations.Filters.Destination.Key] = Ks4Destinations.Filters.Destination.Values.Education,
            ["yyy"] = "2",
        }));

        response.Attainment8.Series.Should().NotBeEmpty();
        response.EnglishMaths.Series.Should().NotBeEmpty();
        response.Destinations.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Attainment8_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var seriesTypes = response.Attainment8.Series.Select(s => s.SeriesType);

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
    public async Task Attainment8_WhenNoPerformanceData_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Attainment8_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithAttainment8(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Attainment8_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 70.0, 65.0, 82.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Attainment8_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithAttainment8(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Attainment8_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Attainment8_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "", prev: "103.1", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "104.0", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "102.0", prev: "101.0", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 103, 102.1m, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Attainment8_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithAttainment8(current: "106.2", prev: "105.4", prev2: "104.1")));

        var response = await _sut.Execute(Request(urn));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "104.2", prev: "102.8", prev2: "101.8")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 104.2m, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Attainment8_TopPerfomers_LimitedToTop3()
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
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks4Performance.Establishment("100003", x => x.WithAttainment8(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks4Performance.Establishment("100004", x => x.WithAttainment8(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks4Performance.Establishment("100005", x => x.WithAttainment8(current: "103.7", prev: "102.9", prev2: "101.9")));

        var response = await _sut.Execute(Request("100001"));

        response.Attainment8.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100003", "Test School 3", 104.2m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task EnglishMaths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.EnglishMaths.Series.Select(s => s.SeriesType);

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
    public async Task EnglishMaths_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishMaths.Series
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
    public async Task EnglishMaths_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishMaths.Series
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
    public async Task EnglishMaths_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishMaths.Series
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
    public async Task EnglishMaths_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task EnglishMaths_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishMaths_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "", prev: "70", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "80", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task EnglishMaths_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks4Performance.LA("001", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task EnglishMaths_TopPerfomers_LimitedToTop3()
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
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngMaths49(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Performance.Establishment("100004", x => x.WithEngMaths49(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Performance.Establishment("100005", x => x.WithEngMaths49(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove)]
    [Theory]
    public async Task EnglishMaths_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishMaths.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishMaths.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishMaths_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "60", prev: "59", prev2: "58")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngMaths49(current: "70", prev: "69", prev2: "68")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks4Performance.LA("001", x => x
                .WithEngMaths49(current: "73", prev: "72", prev2: "71")
                .WithEngMaths59(current: "63", prev: "62", prev2: "61")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "74", prev: "73", prev2: "72")
                .WithEngMaths59(current: "64", prev: "63", prev2: "62")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishMaths.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishMaths.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove, new[] { "100004", "100003", "100002" })]
    [Theory]
    public async Task EnglishMaths_FilterBy_Grade_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
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
                .WithEngMaths49(current: "30", prev: "", prev2: "")
                .WithEngMaths59(current: "96", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "20", prev: "", prev2: "")
                .WithEngMaths59(current: "97", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x
                .WithEngMaths49(current: "10", prev: "", prev2: "")
                .WithEngMaths59(current: "98", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x
                .WithEngMaths49(current: "0", prev: "", prev2: "")
                .WithEngMaths59(current: "99", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4EnglishMaths.Filters.Grade.Key] = subject
        }));

        var topPerformers = response.EnglishMaths.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task Destinations_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Destinations.Series.Select(s => s.SeriesType);

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
    public async Task Destinations_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Destinations.Series
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
    public async Task Destinations_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "", prev: "", prev2: "")));

        _destinationsRepo.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "", prev: "", prev2: "")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Destinations.Series
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
    public async Task Destinations_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")));

        _destinationsRepo.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Destinations.Series
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
    public async Task Destinations_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "80", prev: "70", prev2: "85")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "60", prev: "60", prev2: "80")));

        _destinationsRepo.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task Destinations_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Destinations_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "", prev: "70", prev2: "")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "80", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task Destinations_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("XYZ")));

        _destinationsRepo.SetupLADestinations(
            Build.Ks4Destinations.LA("001", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task Destinations_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task Destinations_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Destinations_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Destinations_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Destinations_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Secondary()),
            Build.Establishment("100002", "Test School AAA", x => x.Secondary()),
            Build.Establishment("100003", "Test School BBB", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "20", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task Destinations_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()),
            Build.Establishment("100005", "Test School 5", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "20", prev: "70", prev2: "50")),
            Build.Ks4Destinations.Establishment("100003", x => x.WithAllDest(current: "21", prev: "69", prev2: "51")),
            Build.Ks4Destinations.Establishment("100004", x => x.WithAllDest(current: "22", prev: "68", prev2: "49")),
            Build.Ks4Destinations.Establishment("100005", x => x.WithAllDest(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks4Destinations.Filters.Destination.Values.Education)]
    [InlineData(Ks4Destinations.Filters.Destination.Values.Employment)]
    [Theory]
    public async Task Destinations_FilterBy_Subject_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")),
            Build.Ks4Destinations.Establishment("100003", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")));

        _destinationsRepo.SetupLADestinations(
             Build.Ks4Destinations.LA("001", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Destinations.Filters.Destination.Key] = subject
        }));

        var series = response.Destinations.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Destinations.Filters.Destination.Values.Education, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.Employment, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.AllDestinations, new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    // Empty or invalid filter values default to ReadingWritingMaths
    [InlineData("", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [InlineData("xyz", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [Theory]
    public async Task Destinations_FilterBy_Subject_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Secondary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithAllDest(current: "82", prev: "81", prev2: "80")
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "60", prev: "59", prev2: "58")),
            Build.Ks4Destinations.Establishment("100003", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "70", prev: "69", prev2: "68")
                .WithEmployment(current: "62", prev: "61", prev2: "60")));

        _destinationsRepo.SetupLADestinations(
             Build.Ks4Destinations.LA("001", x => x
                .WithAllDest(current: "83", prev: "82", prev2: "81")
                .WithEducation(current: "73", prev: "72", prev2: "71")
                .WithEmployment(current: "63", prev: "62", prev2: "61")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithAllDest(current: "84", prev: "83", prev2: "82")
                .WithEducation(current: "74", prev: "73", prev2: "72")
                .WithEmployment(current: "64", prev: "63", prev2: "62")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Destinations.Filters.Destination.Key] = subject
        }));

        var series = response.Destinations.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks4Destinations.Filters.Destination.Values.Education, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.Employment, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.AllDestinations, new[] { "100002", "100003", "100004" })]
    [Theory]
    public async Task Destinations_FilterBy_Subject_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()),
            Build.Establishment("100003", "Test School 3", x => x.Secondary()),
            Build.Establishment("100004", "Test School 4", x => x.Secondary()));

        _similarSchoolsRepo.SetupGroups(
            Build.SecondaryGroup("100001", ["100002", "100003", "100004"]));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithAllDest(current: "1", prev: "", prev2: "")
                .WithEducation(current: "30", prev: "", prev2: "")
                .WithEmployment(current: "96", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithAllDest(current: "4", prev: "", prev2: "")
                .WithEducation(current: "20", prev: "", prev2: "")
                .WithEmployment(current: "97", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100003", x => x
                .WithAllDest(current: "3", prev: "", prev2: "")
                .WithEducation(current: "10", prev: "", prev2: "")
                .WithEmployment(current: "98", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100004", x => x
                .WithAllDest(current: "2", prev: "", prev2: "")
                .WithEducation(current: "0", prev: "", prev2: "")
                .WithEmployment(current: "99", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks4Destinations.Filters.Destination.Key] = subject
        }));

        var topPerformers = response.Destinations.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    private GetSchoolKs4HeadlineMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}
