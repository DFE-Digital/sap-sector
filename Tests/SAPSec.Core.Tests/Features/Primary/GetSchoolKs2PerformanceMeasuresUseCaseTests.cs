using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Constants.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Primary;

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
            Build.Ks2Performance.Establishment("100001", x => x.WithRwmExpected(current: "18", prev: "75", prev2: "80")),
            Build.Ks2Performance.Establishment("100002", x => x.WithRwmExpected(current: "20", prev: "70", prev2: "50")),
            Build.Ks2Performance.Establishment("100003", x => x.WithRwmExpected(current: "21", prev: "69", prev2: "51")),
            Build.Ks2Performance.Establishment("100004", x => x.WithRwmExpected(current: "22", prev: "68", prev2: "49")),
            Build.Ks2Performance.Establishment("100005", x => x.WithRwmExpected(current: "19", prev: "61", prev2: "67")));

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = "1",
            [Ks2ExpectedRwm.Filters.Subject.Key] = Ks2ExpectedRwm.Filters.Subject.Values.Maths,
            ["yyy"] = "2",
        }));

        response.MeetingExpectedStandardRwm.Series.Should().NotBeEmpty();
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

    //[InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Reading, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 73.0, 72.0, 71.0 }, new[] { 74.0, 73.0, 72.0 })]
    //[InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Writing, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 63.0, 62.0, 61.0 }, new[] { 64.0, 63.0, 62.0 })]
    //[InlineData(Ks2ExpectedRwm.Filters.Subject.Values.Maths, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 53.0, 52.0, 51.0 }, new[] { 54.0, 53.0, 52.0 })]
    //[InlineData(Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths, new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    // Empty or invalid filter values default to ReadingWritingMaths
    [InlineData("", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
    //[InlineData("xyz", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 83.0, 82.0, 81.0 }, new[] { 84.0, 83.0, 82.0 })]
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

    private GetSchoolKs2PerformanceMeasuresRequest Request(string urn, Dictionary<string, string>? filterBy = null) =>
            new(urn, filterBy ?? []);
}