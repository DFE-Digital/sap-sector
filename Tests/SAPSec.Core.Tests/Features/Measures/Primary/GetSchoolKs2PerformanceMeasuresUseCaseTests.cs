using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Measures.Primary;

public class GetSchoolKs2PerformanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo;
    private readonly InMemoryKs2PerformanceRepository _performanceRepo;
    private readonly GetSchoolKs2PerformanceMeasuresUseCase _sut;

    public GetSchoolKs2PerformanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _similarSchoolsRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _sut = new GetSchoolKs2PerformanceMeasuresUseCase(
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
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "18", prev: "75", prev2: "80")
                .WithRwmHigher(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpected(current: "20", prev: "70", prev2: "50")
                .WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmExpected(current: "21", prev: "69", prev2: "51")
                .WithRwmHigher(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x
                .WithRwmExpected(current: "22", prev: "68", prev2: "49")
                .WithRwmHigher(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x
                .WithRwmExpected(current: "19", prev: "61", prev2: "67")
                .WithRwmHigher(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Ks2ExpectedRwm.Filters.Subject.Key] = Ks2ExpectedRwm.Filters.Subject.Values.Maths,
            [Ks2HigherRwm.Filters.Subject.Key] = Ks2HigherRwm.Filters.Subject.Values.Maths,
            ["yyy"] = "2",
        }));

        response.MeetingExpectedStandardRwm.Series.Should().NotBeEmpty();
        response.AchievedHigherStandardRwm.Series.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
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

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task MeetingExpectedStandardRwm_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardRwm.Series
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
    public async Task MeetingExpectedStandardRwm_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardRwm.Series
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
    public async Task MeetingExpectedStandardRwm_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardRwm.Series
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
    public async Task MeetingExpectedStandardRwm_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "80", prev: "70", prev2: "85")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmExpected(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
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

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task MeetingExpectedStandardRwm_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmExpected(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.MeetingExpectedStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task MeetingExpectedStandardRwm_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
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
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
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
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
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
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
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
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
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



    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Reading)]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Writing)]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Maths)]
    [Theory]
    public async Task MeetingExpectedStandardRwm_FilterBy_Subject_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedWriting(current: "", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "x", prev: "y", prev2: "z")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedWriting(current: "", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "x", prev: "y", prev2: "z")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedWriting(current: "", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "x", prev: "y", prev2: "z")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedWriting(current: "", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "x", prev: "y", prev2: "z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedWriting(current: "", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "x", prev: "y", prev2: "z")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2ExpectedRwm.Filters.Subject.Key] = subject
        }));

        var series = response.MeetingExpectedStandardRwm.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Reading, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Writing, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Maths, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths, new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    // Empty or invalid filter values default to ReadingWritingMaths
    [InlineData("", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [InlineData("xyz", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [Theory]
    public async Task MeetingExpectedStandardRwm_FilterBy_Subject_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "82", prev: "81", prev2: "80")
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedReading(current: "72", prev: "71", prev2: "70")
                .WithRwmExpectedWriting(current: "60", prev: "59", prev2: "58")
                .WithRwmExpectedMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmExpected(current: "81", prev: "80", prev2: "79")
                .WithRwmExpectedReading(current: "70", prev: "69", prev2: "68")
                .WithRwmExpectedWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmExpectedMaths(current: "50", prev: "49", prev2: "48")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmExpected(current: "83", prev: "82", prev2: "81")
                .WithRwmExpectedReading(current: "73", prev: "72", prev2: "71")
                .WithRwmExpectedWriting(current: "63", prev: "62", prev2: "61")
                .WithRwmExpectedMaths(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmExpected(current: "84", prev: "83", prev2: "82")
                .WithRwmExpectedReading(current: "74", prev: "73", prev2: "72")
                .WithRwmExpectedWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmExpectedMaths(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2ExpectedRwm.Filters.Subject.Key] = subject
        }));

        var series = response.MeetingExpectedStandardRwm.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Reading, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Writing, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Maths, new[] { "100003", "100001", "100002" })]
    [InlineData(Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths, new[] { "100002", "100003", "100004" })]
    [Theory]
    public async Task MeetingExpectedStandardRwm_FilterBy_Subject_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmExpected(current: "1", prev: "", prev2: "")
                .WithRwmExpectedReading(current: "30", prev: "", prev2: "")
                .WithRwmExpectedWriting(current: "96", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "50", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmExpected(current: "4", prev: "", prev2: "")
                .WithRwmExpectedReading(current: "20", prev: "", prev2: "")
                .WithRwmExpectedWriting(current: "97", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "40", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmExpected(current: "3", prev: "", prev2: "")
                .WithRwmExpectedReading(current: "10", prev: "", prev2: "")
                .WithRwmExpectedWriting(current: "98", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "60", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x
                .WithRwmExpected(current: "2", prev: "", prev2: "")
                .WithRwmExpectedReading(current: "0", prev: "", prev2: "")
                .WithRwmExpectedWriting(current: "99", prev: "", prev2: "")
                .WithRwmExpectedMaths(current: "30", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2ExpectedRwm.Filters.Subject.Key] = subject
        }));

        var topPerformers = response.MeetingExpectedStandardRwm.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.AchievedHigherStandardRwm.Series.Select(s => s.SeriesType);

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
    public async Task AchievedHigherStandardRwm_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardRwm.Series
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
    public async Task AchievedHigherStandardRwm_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardRwm.Series
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
    public async Task AchievedHigherStandardRwm_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardRwm.Series
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
    public async Task AchievedHigherStandardRwm_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "81", prev: "80", prev2: "79")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "80", prev: "70", prev2: "85")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "60", prev: "60", prev2: "80")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithRwmHigher(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "", prev: "70", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "80", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "60", prev: "60", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.AchievedHigherStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 70, 65, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task AchievedHigherStandardRwm_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithRwmHigher(current: "71", prev: "70", prev2: "69")));

        var response = await _sut.Execute(Request(urn));

        var series = response.AchievedHigherStandardRwm.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "20", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "20", prev: "68", prev2: "49")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 20, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 20, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 20, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardRwm_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmHigher(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmHigher(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmHigher(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmHigher(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmHigher(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));
        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 22, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 21, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 20, IsCurrentSchool: false)
        ]);
    }

    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Reading)]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Writing)]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Maths)]
    [Theory]
    public async Task AchievedHigherStandardRwm_FilterBy_Subject_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherWriting(current: "", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "x", prev: "y", prev2: "z")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherWriting(current: "", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "x", prev: "y", prev2: "z")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherWriting(current: "", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "x", prev: "y", prev2: "z")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherWriting(current: "", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "x", prev: "y", prev2: "z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherWriting(current: "", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "x", prev: "y", prev2: "z")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2HigherRwm.Filters.Subject.Key] = subject
        }));

        var series = response.AchievedHigherStandardRwm.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Reading, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Writing, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Maths, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.ReadingWritingMaths, new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    // Empty or invalid filter values default to ReadingWritingMaths
    [InlineData("", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [InlineData("xyz", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    [Theory]
    public async Task AchievedHigherStandardRwm_FilterBy_Subject_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] la, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmHigher(current: "82", prev: "81", prev2: "80")
                .WithRwmHigherReading(current: "72", prev: "71", prev2: "70")
                .WithRwmHigherWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmHigherMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherReading(current: "72", prev: "71", prev2: "70")
                .WithRwmHigherWriting(current: "60", prev: "59", prev2: "58")
                .WithRwmHigherMaths(current: "52", prev: "51", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmHigher(current: "81", prev: "80", prev2: "79")
                .WithRwmHigherReading(current: "70", prev: "69", prev2: "68")
                .WithRwmHigherWriting(current: "62", prev: "61", prev2: "60")
                .WithRwmHigherMaths(current: "50", prev: "49", prev2: "48")));

        _performanceRepo.SetupLAPerformance(
             Build.Ks2Performance.LA("001", x => x
                .WithRwmHigher(current: "83", prev: "82", prev2: "81")
                .WithRwmHigherReading(current: "73", prev: "72", prev2: "71")
                .WithRwmHigherWriting(current: "63", prev: "62", prev2: "61")
                .WithRwmHigherMaths(current: "53", prev: "52", prev2: "51")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x
                .WithRwmHigher(current: "84", prev: "83", prev2: "82")
                .WithRwmHigherReading(current: "74", prev: "73", prev2: "72")
                .WithRwmHigherWriting(current: "64", prev: "63", prev2: "62")
                .WithRwmHigherMaths(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2HigherRwm.Filters.Subject.Key] = subject
        }));

        var series = response.AchievedHigherStandardRwm.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, (decimal?)la[0], (decimal?)la[1], (decimal?)la[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Reading, new[] { "100001", "100002", "100003" })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Writing, new[] { "100004", "100003", "100002" })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.Maths, new[] { "100003", "100001", "100002" })]
    [InlineData(Ks2HigherRwm.Filters.Subject.Values.ReadingWritingMaths, new[] { "100002", "100003", "100004" })]
    [Theory]
    public async Task AchievedHigherStandardRwm_FilterBy_Subject_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValueForSelectedSubject(string subject, string[] expected)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithRwmHigher(current: "1", prev: "", prev2: "")
                .WithRwmHigherReading(current: "30", prev: "", prev2: "")
                .WithRwmHigherWriting(current: "96", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "50", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithRwmHigher(current: "4", prev: "", prev2: "")
                .WithRwmHigherReading(current: "20", prev: "", prev2: "")
                .WithRwmHigherWriting(current: "97", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "40", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithRwmHigher(current: "3", prev: "", prev2: "")
                .WithRwmHigherReading(current: "10", prev: "", prev2: "")
                .WithRwmHigherWriting(current: "98", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "60", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x
                .WithRwmHigher(current: "2", prev: "", prev2: "")
                .WithRwmHigherReading(current: "0", prev: "", prev2: "")
                .WithRwmHigherWriting(current: "99", prev: "", prev2: "")
                .WithRwmHigherMaths(current: "30", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [Ks2HigherRwm.Filters.Subject.Key] = subject
        }));

        var topPerformers = response.AchievedHigherStandardRwm.TopPerformers;

        topPerformers.Should().NotBeNullOrEmpty();
        topPerformers.Select(tp => tp.Urn).Should().Equal(expected);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var seriesTypes = response.MeetingExpectedStandardGps.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 62.0, 61.0, 60.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 76.5, 75.5, 74.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 73.0, 72.0, 71.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 69.0, 68.0, 67.0)]
    [Theory]
    public async Task MeetingExpectedStandardGps_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "75", prev2: "74")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsExpected(current: "73", prev: "72", prev2: "71")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "69", prev: "68", prev2: "67")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task MeetingExpectedStandardGps_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task MeetingExpectedStandardGps_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsExpected(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task MeetingExpectedStandardGps_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsExpected(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsExpected(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "", prev: "76", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "78", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 77, 75, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task MeetingExpectedStandardGps_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsExpected(current: "73", prev: "72", prev2: "71")));

        var response = await _sut.Execute(Request(urn));

        response.MeetingExpectedStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "73")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 76, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 62, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "", prev: "75", prev2: "74")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "73")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 76, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 62, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "74", prev2: "73")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School 2", 77, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 76, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 62, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "61", prev2: "60")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "76", prev2: "75")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "77", prev: "75", prev2: "74")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 77, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 77, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 62, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "75", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "76", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "77", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsExpected(current: "74", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 77m, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 76m, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 75m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task MeetingExpectedStandardGps_TopPerfomers_LimitedToTop3_AndTiedValuesSortAlphabetically()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School B", x => x.Primary()),
            Build.Establishment("100003", "Test School A", x => x.Primary()),
            Build.Establishment("100004", "Test School C", x => x.Primary()),
            Build.Establishment("100005", "Test School D", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsExpected(current: "62", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsExpected(current: "77", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsExpected(current: "77", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsExpected(current: "76", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsExpected(current: "75", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.MeetingExpectedStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School A", 77m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School B", 77m, IsCurrentSchool: false),
            new TopPerformer(3, "100004", "Test School C", 76m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var seriesTypes = response.AchievedHigherStandardGps.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 18.0, 17.0, 16.0)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage, 23.5, 22.5, 21.5)]
    [InlineData(MeasureSeriesType.LASchoolsAverage, 19.0, 18.0, 17.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 15.0, 14.0, 13.0)]
    [Theory]
    public async Task AchievedHigherStandardGps_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "22", prev2: "21")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsHigher(current: "19", prev: "18", prev2: "17")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "15", prev: "14", prev2: "13")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task AchievedHigherStandardGps_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task AchievedHigherStandardGps_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsHigher(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task AchievedHigherStandardGps_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsHigher(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithGpsHigher(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        var series = response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "", prev: "23", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "24", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "22", prev: "21", prev2: "")));

        var response = await _sut.Execute(Request("100001"));
        var series = response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage);

        series.Should().NotBeNull();
        series.Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 23, 22, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task AchievedHigherStandardGps_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithGpsHigher(current: "19", prev: "18", prev2: "17")));

        var response = await _sut.Execute(Request(urn));

        response.AchievedHigherStandardGps.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "21", prev2: "20")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 23, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 18, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "21", prev2: "20")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 23, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 18, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "21", prev2: "20")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School 2", 24, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 23, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 18, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "17", prev2: "16")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "23", prev2: "22")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "24", prev: "22", prev2: "21")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 24, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 24, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 18, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "22", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "23", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "24", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsHigher(current: "21", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 24m, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School 3", 23m, IsCurrentSchool: false),
            new TopPerformer(3, "100002", "Test School 2", 22m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_LimitedToTop3_AndTiedValuesSortAlphabetically()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School B", x => x.Primary()),
            Build.Establishment("100003", "Test School A", x => x.Primary()),
            Build.Establishment("100004", "Test School C", x => x.Primary()),
            Build.Establishment("100005", "Test School D", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "24", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "24", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "23", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100005", x => x.WithGpsHigher(current: "22", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School A", 24m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School B", 24m, IsCurrentSchool: false),
            new TopPerformer(3, "100004", "Test School C", 23m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task AchievedHigherStandardGps_TopPerfomers_WhenDisplayedValuesTie_SortsAlphabetically()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Thoresby Primary School", x => x.Primary()),
            Build.Establishment("100003", "Manor Park Primary Academy", x => x.Primary()),
            Build.Establishment("100004", "Montem Academy", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithGpsHigher(current: "18", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100002", x => x.WithGpsHigher(current: "96.6", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithGpsHigher(current: "96.5", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithGpsHigher(current: "91.4", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AchievedHigherStandardGps.TopPerformers.Should().Equal(
            new TopPerformer(1, "100003", "Manor Park Primary Academy", 96.5m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Thoresby Primary School", 96.6m, IsCurrentSchool: false),
            new TopPerformer(3, "100004", "Montem Academy", 91.4m, IsCurrentSchool: false));
    }

    [Fact]
    public async Task AverageScaledScoreReading_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
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
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "107.2", prev: "106.3", prev2: "105.1")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, 107.2m, 106.3m, 105.1m));
    }

    [Fact]
    public async Task AverageScaledScoreReading_CurrentSchool_WhenCurrentSchoolHasNoPerformanceData_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreReading_CurrentSchool_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task AverageScaledScoreReading_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithReadingScaledScore(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreReading_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreReading_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "", prev: "103.1", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.0", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "102.0", prev: "101.0", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 103, 102.1m, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task AverageScaledScoreReading_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithReadingScaledScore(current: "106.2", prev: "105.4", prev2: "104.1")));

        var response = await _sut.Execute(Request(urn));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreReading_SimilarSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "106.4", prev: "103.1", prev2: "101.2")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.0", prev: "102.3", prev2: "99.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 105.2m, 102.7m, 100.5m));
    }

    [Fact]
    public async Task AverageScaledScoreReading_LASchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithReadingScaledScore(current: "106.2", prev: "105.4", prev2: "104.1")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, 106.2m, 105.4m, 104.1m));
    }

    [Fact]
    public async Task AverageScaledScoreReading_EnglandSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithReadingScaledScore(current: "107.4", prev: "106.6", prev2: "105.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 107.4m, 106.6m, 105.8m));
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.2", prev: "102.8", prev2: "101.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 104.2m, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 101.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreReading_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithReadingScaledScore(current: "101.1", prev: "100.5", prev2: "99.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithReadingScaledScore(current: "104.2", prev: "103.1", prev2: "102.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithReadingScaledScore(current: "104.2", prev: "102.8", prev2: "101.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithReadingScaledScore(current: "106.3", prev: "105.4", prev2: "104.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithReadingScaledScore(current: "103.7", prev: "102.9", prev2: "101.9")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreReading.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 106.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 104.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100003", "Test School 3", 104.2m, IsCurrentSchool: false)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        var seriesTypes = response.AverageScaledScoreMaths.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchoolsAverage,
            MeasureSeriesType.LASchoolsAverage,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_CurrentSchool_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "108.2", prev: "107.3", prev2: "106.1")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, 108.2m, 107.3m, 106.1m));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_CurrentSchool_WhenCurrentSchoolHasNoPerformanceData_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_CurrentSchool_WhenEmptyValues_ContainsNulls()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.CurrentSchool)
            .Should().Be(new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchoolsAverage)]
    [InlineData(MeasureSeriesType.LASchoolsAverage)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task AverageScaledScoreMaths_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("001")),
            Build.Establishment("100004", "Test School 4", x => x.Primary().InLA("001")));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks2Performance.Establishment("100004", x => x.WithMathsScaledScore(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithMathsScaledScore(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_SimilarSchoolsAverage_WhenNoSimilarSchoolsForCurrentSchool_ContainsNullValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_SimilarSchoolsAverage_WhenEmptyValuesPresent_CalculatesAverageOfRemainingValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "", prev: "104.1", prev2: "")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.0", prev: "", prev2: "")),
            Build.Ks2Performance.Establishment("100004", x => x.WithMathsScaledScore(current: "103.0", prev: "102.0", prev2: "")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 104, 103.1m, null));
    }

    [InlineData("100001")]
    [InlineData("100002")]
    [InlineData("100003")]
    [Theory]
    public async Task AverageScaledScoreMaths_LASchoolsAverage_WhenLAIdMissingOrInvalid_ContainsNullValues(string urn)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary().InLA("002")),
            Build.Establishment("100003", "Test School 3", x => x.Primary().InLA("XYZ")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithMathsScaledScore(current: "107.2", prev: "106.4", prev2: "105.1")));

        var response = await _sut.Execute(Request(urn));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, null, null, null));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_SimilarSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "107.4", prev: "104.1", prev2: "102.2")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.0", prev: "103.3", prev2: "100.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.SimilarSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 106.2m, 103.7m, 101.5m));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_LASchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary().InLA("001")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithMathsScaledScore(current: "107.2", prev: "106.4", prev2: "105.1")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.LASchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.LASchoolsAverage, 107.2m, 106.4m, 105.1m));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_EnglandSchoolsAverage_ContainsYearByYearValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithMathsScaledScore(current: "108.4", prev: "107.6", prev2: "106.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.Series
            .FirstOrDefault(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage)
            .Should().Be(new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 108.4m, 107.6m, 106.8m));
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_WhenNoPerformanceDataForSimilarSchools_IsEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEmpty();
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_WhenNoPerformanceDataForSchool_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 107.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 102.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_WhenNoPerformanceDataForSchoolForCurrentYear_SchoolDoesNotAppear()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 107.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100001", "Test School 1", 102.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_RanksSimilarSchoolsBasedOnCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "105.2", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100003", "Test School 3", 107.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 105.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School 1", 102.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_RanksSimilarSchoolsBasedOnNameIfSameCurrentYearValue()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School CCC", x => x.Primary()),
            Build.Establishment("100002", "Test School AAA", x => x.Primary()),
            Build.Establishment("100003", "Test School BBB", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "105.2", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.2", prev: "103.8", prev2: "102.8")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Test School AAA", 105.2m, IsCurrentSchool: false),
            new TopPerformer(2, "100003", "Test School BBB", 105.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100001", "Test School CCC", 102.1m, IsCurrentSchool: true)
        ]);
    }

    [Fact]
    public async Task AverageScaledScoreMaths_TopPerfomers_LimitedToTop3()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Primary()),
            Build.Establishment("100002", "Test School 2", x => x.Primary()),
            Build.Establishment("100003", "Test School 3", x => x.Primary()),
            Build.Establishment("100004", "Test School 4", x => x.Primary()),
            Build.Establishment("100005", "Test School 5", x => x.Primary()));

        _similarSchoolsRepo.SetupGroups(
            Build.PrimaryGroup("100001", ["100002", "100003", "100004", "100005"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x.WithMathsScaledScore(current: "102.1", prev: "101.5", prev2: "100.5")),
            Build.Ks2Performance.Establishment("100002", x => x.WithMathsScaledScore(current: "105.2", prev: "104.1", prev2: "103.1")),
            Build.Ks2Performance.Establishment("100003", x => x.WithMathsScaledScore(current: "105.2", prev: "103.8", prev2: "102.8")),
            Build.Ks2Performance.Establishment("100004", x => x.WithMathsScaledScore(current: "107.3", prev: "106.4", prev2: "105.4")),
            Build.Ks2Performance.Establishment("100005", x => x.WithMathsScaledScore(current: "104.7", prev: "103.9", prev2: "102.9")));

        var response = await _sut.Execute(Request("100001"));

        response.AverageScaledScoreMaths.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100004", "Test School 4", 107.3m, IsCurrentSchool: false),
            new TopPerformer(2, "100002", "Test School 2", 105.2m, IsCurrentSchool: false),
            new TopPerformer(3, "100003", "Test School 3", 105.2m, IsCurrentSchool: false)
        ]);
    }

    private GetSchoolKs2PerformanceMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}
