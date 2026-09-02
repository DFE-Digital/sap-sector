using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Measures.Primary;

public class GetComparisonKs2PerformanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo;
    private readonly InMemoryKs2PerformanceRepository _performanceRepo;
    private readonly GetComparisonKs2PerformanceMeasuresUseCase _sut;

    public GetComparisonKs2PerformanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _similarSchoolsRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _sut = new GetComparisonKs2PerformanceMeasuresUseCase(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo);
    }

    private static GetComparisonKs2PerformanceMeasuresRequest Request(
        string urn,
        string similarSchoolUrn,
        Dictionary<string, string>? filterBy = null) =>
        new(urn, similarSchoolUrn, filterBy);

    [Fact]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        var act = async () => await _sut.Execute(Request("999999", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenComparatorSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()));

        var act = async () => await _sut.Execute(Request("100001", "999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School"),
            Build.Establishment("100002", "Comparator School"));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", []));

        var act = async () => await _sut.Execute(Request("100001", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100002*");
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.MeetingExpectedStandardRwm.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "60", prev: "61", prev2: "62")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.MeetingExpectedStandardRwm.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 81m, 80m, 79m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 60m, 61m, 62m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 61m, 60m, 59m)
        ]);
    }

    [Theory]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Reading, 72.0, 52.0, 74.0)]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Writing, 62.0, 42.0, 64.0)]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Maths, 52.0, 32.0, 54.0)]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths, 82.0, 62.0, 84.0)]
    public async Task MeetingExpectedStandardRwm_FilterBy_Subject_ContainsCurrentYearValuesForSelectedSubject(
        string subject, double currentSchool, double similarSchool, double england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "82", prev: "81", prev2: "80")
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpected(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedReading(current: "52", prev: "51", prev2: "50")
                .WithRwmExpectedWriting(current: "42", prev: "41", prev2: "40")
                .WithRwmExpectedMaths(current: "32", prev: "31", prev2: "30")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmExpected(current: "84", prev: "83", prev2: "82")
                .WithRwmExpectedReading(current: "74", prev: "73", prev2: "72")
                .WithRwmExpectedWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmExpectedMaths(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", new()
        {
            [Ks2ExpectedRwm.Filters.Subject.Key] = subject
        }));

        var series = response.MeetingExpectedStandardRwm.Series;

        series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be((decimal?)currentSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool).Current.Should().Be((decimal?)similarSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be((decimal?)england);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.MeetingExpectedStandardRwm.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.AchievedHigherStandardRwm.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "31", prev: "30", prev2: "29")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "20", prev: "21", prev2: "22")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "21", prev: "20", prev2: "19")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AchievedHigherStandardRwm.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 31m, 30m, 29m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 20m, 21m, 22m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 21m, 20m, 19m)
        ]);
    }

    [Theory]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Reading, 22.0, 12.0, 24.0)]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Writing, 17.0, 7.0, 19.0)]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Maths, 12.0, 2.0, 14.0)]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.ReadingWritingMaths, 32.0, 22.0, 34.0)]
    public async Task AchievedHigherStandardRwm_FilterBy_Subject_ContainsCurrentYearValuesForSelectedSubject(
        string subject, double currentSchool, double similarSchool, double england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmHigher(current: "32", prev: "31", prev2: "30")
                .WithRwmHigherReading(current: "22", prev: "21", prev2: "20")
                .WithRwmHigherWriting(current: "17", prev: "16", prev2: "15")
                .WithRwmHigherMaths(current: "12", prev: "11", prev2: "10")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmHigher(current: "22", prev: "21", prev2: "20")
                .WithRwmHigherReading(current: "12", prev: "11", prev2: "10")
                .WithRwmHigherWriting(current: "7", prev: "6", prev2: "5")
                .WithRwmHigherMaths(current: "2", prev: "1", prev2: "0")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmHigher(current: "34", prev: "33", prev2: "32")
                .WithRwmHigherReading(current: "24", prev: "23", prev2: "22")
                .WithRwmHigherWriting(current: "19", prev: "18", prev2: "17")
                .WithRwmHigherMaths(current: "14", prev: "13", prev2: "12")));

        var response = await _sut.Execute(Request("100001", "100002", new()
        {
            [Ks2HigherRwm.Filters.Subject.Key] = subject
        }));

        var series = response.AchievedHigherStandardRwm.Series;

        series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be((decimal?)currentSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool).Current.Should().Be((decimal?)similarSchool);
        series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage).Current.Should().Be((decimal?)england);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "31", prev: "30", prev2: "29")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AchievedHigherStandardRwm.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreReading_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.AverageScaledScoreReading.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.4", prev: "100.4", prev2: "99.4")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "103.2", prev: "102.2", prev2: "101.2")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.4", prev: "106.6", prev2: "105.8")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AverageScaledScoreReading.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 101.4m, 100.4m, 99.4m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 103.2m, 102.2m, 101.2m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 107.4m, 106.6m, 105.8m)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.4", prev: "100.4", prev2: "99.4")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AverageScaledScoreReading.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.AverageScaledScoreMaths.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "104.2", prev: "103.2", prev2: "102.2")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AverageScaledScoreMaths.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 102.4m, 101.4m, 100.4m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 104.2m, 103.2m, 102.2m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 108.4m, 107.6m, 106.8m)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.4", prev: "101.4", prev2: "100.4")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AverageScaledScoreMaths.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.MeetingExpectedStandardGps.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "69", prev: "68", prev2: "67")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.MeetingExpectedStandardGps.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 62m, 61m, 60m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 77m, 76m, 75m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 69m, 68m, 67m)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.MeetingExpectedStandardGps.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.AchievedHigherStandardGps.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.ComparatorSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ContainsYearByYearValuesForCurrentAndComparatorSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "15", prev: "14", prev2: "13")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AchievedHigherStandardGps.Series;

        series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 18m, 17m, 16m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 24m, 23m, 22m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 15m, 14m, 13m)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_WhenNoPerformanceDataForComparatorSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")));

        var response = await _sut.Execute(Request("100001", "100002"));
        var series = response.AchievedHigherStandardGps.Series
            .First(s => s.SeriesType == MeasureSeriesType.ComparatorSchool);

        series.Should().Be(new MeasureSeries(MeasureSeriesType.ComparatorSchool, null, null, null));
    }

    [Fact]
    public async Task FilterBy_ForOneMeasure_DoesNotAffectTheOther()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "82", prev: "81", prev2: "80")
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmHigher(current: "32", prev: "31", prev2: "30")
                .WithRwmHigherReading(current: "22", prev: "21", prev2: "20")));

        var response = await _sut.Execute(Request("100001", "100002", new()
        {
            [Ks2ExpectedRwm.Filters.Subject.Key] = Ks2ExpectedRwm.Filters.Subject.Values.Reading,
            [Ks2HigherRwm.Filters.Subject.Key] = Ks2HigherRwm.Filters.Subject.Values.Reading
        }));

        response.MeetingExpectedStandardRwm.Series
            .First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be(72m);
        response.AchievedHigherStandardRwm.Series
            .First(s => s.SeriesType == MeasureSeriesType.CurrentSchool).Current.Should().Be(22m);
    }
}
