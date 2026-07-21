using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.Primary;

public class GetSchoolKs2PerformanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepository;
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepository;
    private readonly InMemoryKs2PerformanceRepository _performanceRepository;
    private readonly GetSchoolKs2PerformanceMeasuresUseCase _sut;

    public GetSchoolKs2PerformanceMeasuresUseCaseTests()
    {
        _establishmentRepository = new();
        _similarSchoolsRepository = new();
        _performanceRepository = new(_establishmentRepository);
        _sut = new GetSchoolKs2PerformanceMeasuresUseCase(
            _establishmentRepository,
            _similarSchoolsRepository,
            _performanceRepository);
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
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .Primary()
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
    public async Task MeetingExpectedStandardRwm_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.MeetingExpectedStandardRwm.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var seriesTypes = response.AverageScaledScoreReading.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_CurrentSchool_ContainsYearByYearValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScore(current: "107.2", prev: "106.3", prev2: "105.1")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 107.2m, 106.3m, 105.1m));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_CurrentSchool_WhenCurrentSchoolHasNoPerformanceData_ContainsNullValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");

        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_CurrentSchool_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_CurrentSchool_ContainsYearByYearValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, 81, 80, 79));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenNoPerformanceDataForSimilarSchools_ContainsNullValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "80", prev: "70", prev2: "85")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "60", prev: "60", prev2: "80")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, 82.5m));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "", prev: "70", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "80", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_LASchoolsAverage_WhenNoPerformanceDataForLA_ContainsNullValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_LASchoolsAverage_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _performanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_LASchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _performanceRepository.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, 71, 70, 69));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_EnglandSchoolsAverage_WhenNoPerformanceDataForNational_ContainsNullValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_EnglandSchoolsAverage_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _performanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_EnglandSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _performanceRepository.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 71, 70, 69));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepository.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepository.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    private GetSchoolKs2PerformanceMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}
