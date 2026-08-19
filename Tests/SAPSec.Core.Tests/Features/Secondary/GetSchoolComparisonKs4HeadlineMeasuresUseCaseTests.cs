using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.Secondary;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Constants.Measures.Secondary;

namespace SAPSec.Core.Tests.Features.Secondary;

public class GetSchoolComparisonKs4HeadlineMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryKs4PerformanceRepository _performanceRepo;
    private readonly InMemoryKs4DestinationsRepository _destinationsRepo;
    private readonly GetSchoolComparisonKs4HeadlineMeasuresUseCase _sut;

    public GetSchoolComparisonKs4HeadlineMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _destinationsRepo = new(_establishmentRepo);
        _sut = new GetSchoolComparisonKs4HeadlineMeasuresUseCase(
            _establishmentRepo,
            _performanceRepo,
            _destinationsRepo);
    }

    [Fact]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var act = async () => await _sut.Execute(Request("999999", "100002"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenSimilarSchoolDoesNotExist_ThrowsNotFoundException()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var act = async () => await _sut.Execute(Request("100001", "999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task School_ShouldContainCurrentSchoolInfo()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x
                .Secondary()
                .WithAddress("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG")
                .InLA("001", "Test LA")),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.School.Urn.Should().Be("100001");
        response.School.Name.Should().Be("Test School 1");
        response.School.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.School.LocalAuthority.Should().Be(
            new LocalAuthority("001", "Test LA"));
    }

    [Fact]
    public async Task School_ShouldContainSimilarSchoolInfo()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x
                .Secondary()
                .WithAddress("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG")
                .InLA("001", "Test LA")));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.SimilarSchool.Urn.Should().Be("100002");
        response.SimilarSchool.Name.Should().Be("Test School 2");
        response.SimilarSchool.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.SimilarSchool.LocalAuthority.Should().Be(
            new LocalAuthority("001", "Test LA"));
    }

    [Fact]
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithAttainment8(current: "18", prev: "75", prev2: "80")
                .WithEngMaths49(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithAttainment8(current: "20", prev: "70", prev2: "50")
                .WithEngMaths49(current: "20", prev: "70", prev2: "50")));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithEducation(current: "18", prev: "75", prev2: "80")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithEducation(current: "20", prev: "70", prev2: "50")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
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
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var seriesTypes = response.Attainment8.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Attainment8_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Attainment8_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Attainment8_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType)
            .Should().Be(new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Attainment8_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithAttainment8(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithAttainment8(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithAttainment8(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Attainment8.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [Fact]
    public async Task EnglishMaths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.School.Name.Should().Be("Test School 1");
        var seriesTypes = response.EnglishMaths.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishMaths_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishMaths_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishMaths_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task EnglishMaths_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngMaths49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngMaths49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishMaths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove)]
    [Theory]
    public async Task EnglishMaths_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "x", prev: "y", prev2: "z")
                .WithEngMaths59(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishMaths.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishMaths.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishMaths.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishMaths_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngMaths49(current: "72", prev: "71", prev2: "70")
                .WithEngMaths59(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngMaths49(current: "71", prev: "70", prev2: "69")
                .WithEngMaths59(current: "61", prev: "60", prev2: "59")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngMaths49(current: "74", prev: "73", prev2: "72")
                .WithEngMaths59(current: "64", prev: "63", prev2: "62")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishMaths.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishMaths.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Destinations_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.School.Name.Should().Be("Test School 1");
        var seriesTypes = response.Destinations.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
            MeasureSeriesType.SimilarSchool,
            MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Destinations_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Destinations_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "", prev: "", prev2: "")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "", prev: "", prev2: "")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Destinations_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Destinations_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x.WithAllDest(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Destinations.Establishment("100002", x => x.WithAllDest(current: "71", prev: "70", prev2: "69")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x.WithAllDest(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Destinations.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4Destinations.Filters.Destination.Values.Education)]
    [InlineData(Ks4Destinations.Filters.Destination.Values.Employment)]
    [Theory]
    public async Task Destinations_FilterBy_Subject_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "", prev: "", prev2: "")
                .WithEmployment(current: "x", prev: "y", prev2: "z")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Destinations.Filters.Destination.Key] = subject
        }));

        var series = response.Destinations.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Destinations.Filters.Destination.Values.Education, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.Employment, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Destinations.Filters.Destination.Values.AllDestinations, new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 84.0, 83.0, 82.0 })]
    // Empty or invalid filter values default to ReadingWritingMaths
    [InlineData("", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 84.0, 83.0, 82.0 })]
    [InlineData("xyz", new[] { 82.0, 81.0, 80.0 }, new[] { 81.0, 80.0, 79.0 }, new[] { 84.0, 83.0, 82.0 })]
    [Theory]
    public async Task Destinations_FilterBy_Subject_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _destinationsRepo.SetupEstablishmentDestinations(
            Build.Ks4Destinations.Establishment("100001", x => x
                .WithAllDest(current: "82", prev: "81", prev2: "80")
                .WithEducation(current: "72", prev: "71", prev2: "70")
                .WithEmployment(current: "62", prev: "61", prev2: "60")),
            Build.Ks4Destinations.Establishment("100002", x => x
                .WithAllDest(current: "81", prev: "80", prev2: "79")
                .WithEducation(current: "71", prev: "70", prev2: "69")
                .WithEmployment(current: "61", prev: "60", prev2: "59")));

        _destinationsRepo.SetupEnglandDestinations(
            Build.Ks4Destinations.England(x => x
                .WithAllDest(current: "84", prev: "83", prev2: "82")
                .WithEducation(current: "74", prev: "73", prev2: "72")
                .WithEmployment(current: "64", prev: "63", prev2: "62")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Destinations.Filters.Destination.Key] = subject
        }));

        var series = response.Destinations.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    private GetSchoolComparisonKs4HeadlineMeasuresRequest Request(string currentSchoolUrn, string similarSchoolUrn, Dictionary<string, string>? filterBy = null) =>
            new(currentSchoolUrn, similarSchoolUrn, filterBy ?? []);
}
