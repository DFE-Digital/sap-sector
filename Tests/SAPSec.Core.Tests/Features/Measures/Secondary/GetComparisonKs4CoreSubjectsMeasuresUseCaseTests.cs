using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Secondary;

namespace SAPSec.Core.Tests.Features.Measures.Secondary;

public class GetComparisonKs4CoreSubjectsMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryKs4PerformanceRepository _performanceRepo;
    private readonly GetComparisonKs4CoreSubjectsMeasuresUseCase _sut;

    public GetComparisonKs4CoreSubjectsMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _performanceRepo = new(_establishmentRepo);
        _sut = new GetComparisonKs4CoreSubjectsMeasuresUseCase(
            _establishmentRepo,
            _performanceRepo);
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

        response.CurrentSchool.Urn.Should().Be("100001");
        response.CurrentSchool.Name.Should().Be("Test School 1");
        response.CurrentSchool.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.CurrentSchool.LocalAuthority.Should().Be(
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

        response.ComparatorSchool.Urn.Should().Be("100002");
        response.ComparatorSchool.Name.Should().Be("Test School 2");
        response.ComparatorSchool.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.ComparatorSchool.LocalAuthority.Should().Be(
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
                .WithPhysics49(current: "20", prev: "70", prev2: "50")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
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
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.EnglishLanguage.Series.Select(s => s.SeriesType);

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
    public async Task EnglishLanguage_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLanguage_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLanguage_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task EnglishLanguage_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLang49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLang49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLang49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLanguage.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task EnglishLanguage_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLang49(current: "x", prev: "y", prev2: "z")
                .WithEngLang59(current: "", prev: "", prev2: "")
                .WithEngLang79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishLanguage.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLanguage.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4EnglishLanguage.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishLanguage_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchool, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLang49(current: "72", prev: "71", prev2: "70")
                .WithEngLang59(current: "62", prev: "61", prev2: "60")
                .WithEngLang79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLang49(current: "71", prev: "70", prev2: "69")
                .WithEngLang59(current: "61", prev: "60", prev2: "59")
                .WithEngLang79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLang49(current: "74", prev: "73", prev2: "72")
                .WithEngLang59(current: "64", prev: "63", prev2: "62")
                .WithEngLang79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishLanguage.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLanguage.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchool[0], (decimal?)similarSchool[1], (decimal?)similarSchool[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task EnglishLiterature_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.EnglishLiterature.Series.Select(s => s.SeriesType);

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
    public async Task EnglishLiterature_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLiterature_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task EnglishLiterature_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task EnglishLiterature_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithEngLit49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithEngLit49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithEngLit49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.EnglishLiterature.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task EnglishLiterature_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLit49(current: "x", prev: "y", prev2: "z")
                .WithEngLit59(current: "", prev: "", prev2: "")
                .WithEngLit79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishLiterature.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLiterature.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4EnglishLiterature.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task EnglishLiterature_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchool, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithEngLit49(current: "72", prev: "71", prev2: "70")
                .WithEngLit59(current: "62", prev: "61", prev2: "60")
                .WithEngLit79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithEngLit49(current: "71", prev: "70", prev2: "69")
                .WithEngLit59(current: "61", prev: "60", prev2: "59")
                .WithEngLit79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithEngLit49(current: "74", prev: "73", prev2: "72")
                .WithEngLit59(current: "64", prev: "63", prev2: "62")
                .WithEngLit79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4EnglishLiterature.Filters.Grade.Key] = subject
        }));

        var series = response.EnglishLiterature.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchool[0], (decimal?)similarSchool[1], (decimal?)similarSchool[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Maths_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.Maths.Series.Select(s => s.SeriesType);

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
    public async Task Maths_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Maths_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Maths_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Maths_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithMaths49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithMaths49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithMaths49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Maths.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4Maths.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Maths_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithMaths49(current: "x", prev: "y", prev2: "z")
                .WithMaths59(current: "", prev: "", prev2: "")
                .WithMaths79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Maths.Filters.Grade.Key] = subject
        }));

        var series = response.Maths.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Maths.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Maths.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Maths_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithMaths49(current: "72", prev: "71", prev2: "70")
                .WithMaths59(current: "62", prev: "61", prev2: "60")
                .WithMaths79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithMaths49(current: "71", prev: "70", prev2: "69")
                .WithMaths59(current: "61", prev: "60", prev2: "59")
                .WithMaths79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithMaths49(current: "74", prev: "73", prev2: "72")
                .WithMaths59(current: "64", prev: "63", prev2: "62")
                .WithMaths79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Maths.Filters.Grade.Key] = subject
        }));

        var series = response.Maths.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task CombinedScience_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.CombinedScience.Series.Select(s => s.SeriesType);

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
    public async Task CombinedScience_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task CombinedScience_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100004", x => x.WithCombSci49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task CombinedScience_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100003", x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task CombinedScience_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithCombSci49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithCombSci49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithCombSci49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.CombinedScience.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove)]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove)]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove)]
    [Theory]
    public async Task CombinedScience_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithCombSci49(current: "x", prev: "y", prev2: "z")
                .WithCombSci59(current: "", prev: "", prev2: "")
                .WithCombSci79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4CombinedScience.Filters.Grade.Key] = subject
        }));

        var series = response.CombinedScience.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade44AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade55AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4CombinedScience.Filters.Grade.Values.Grade77AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task CombinedScience_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithCombSci49(current: "72", prev: "71", prev2: "70")
                .WithCombSci59(current: "62", prev: "61", prev2: "60")
                .WithCombSci79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithCombSci49(current: "71", prev: "70", prev2: "69")
                .WithCombSci59(current: "61", prev: "60", prev2: "59")
                .WithCombSci79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithCombSci49(current: "74", prev: "73", prev2: "72")
                .WithCombSci59(current: "64", prev: "63", prev2: "62")
                .WithCombSci79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4CombinedScience.Filters.Grade.Key] = subject
        }));

        var series = response.CombinedScience.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Biology_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.Biology.Series.Select(s => s.SeriesType);

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
    public async Task Biology_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Biology_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Biology_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Biology_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithBio49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithBio49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithBio49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Biology.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4Biology.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Biology_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithBio49(current: "x", prev: "y", prev2: "z")
                .WithBio59(current: "", prev: "", prev2: "")
                .WithBio79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Biology.Filters.Grade.Key] = subject
        }));

        var series = response.Biology.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Biology.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Biology.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Biology_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithBio49(current: "72", prev: "71", prev2: "70")
                .WithBio59(current: "62", prev: "61", prev2: "60")
                .WithBio79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithBio49(current: "71", prev: "70", prev2: "69")
                .WithBio59(current: "61", prev: "60", prev2: "59")
                .WithBio79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithBio49(current: "74", prev: "73", prev2: "72")
                .WithBio59(current: "64", prev: "63", prev2: "62")
                .WithBio79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Biology.Filters.Grade.Key] = subject
        }));

        var series = response.Biology.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Chemistry_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.Chemistry.Series.Select(s => s.SeriesType);

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
    public async Task Chemistry_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Chemistry_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Chemistry_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Chemistry_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithChem49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithChem49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithChem49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Chemistry.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Chemistry_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithChem49(current: "x", prev: "y", prev2: "z")
                .WithChem59(current: "", prev: "", prev2: "")
                .WithChem79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Chemistry.Filters.Grade.Key] = subject
        }));

        var series = response.Chemistry.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Chemistry.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Chemistry_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithChem49(current: "72", prev: "71", prev2: "70")
                .WithChem59(current: "62", prev: "61", prev2: "60")
                .WithChem79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithChem49(current: "71", prev: "70", prev2: "69")
                .WithChem59(current: "61", prev: "60", prev2: "59")
                .WithChem79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithChem49(current: "74", prev: "73", prev2: "72")
                .WithChem59(current: "64", prev: "63", prev2: "62")
                .WithChem79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Chemistry.Filters.Grade.Key] = subject
        }));

        var series = response.Chemistry.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    [Fact]
    public async Task Physics_ShouldContainExpectedMeasureSeries()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        response.CurrentSchool.Name.Should().Be("Test School 1");
        var seriesTypes = response.Physics.Series.Select(s => s.SeriesType);

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
    public async Task Physics_WhenNoPerformanceData_ContainsNullValues(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Physics_WhenEmptyValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "", prev: "", prev2: "")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "", prev: "", prev2: "")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "", prev: "", prev2: "")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool)]
    [InlineData(MeasureSeriesType.SimilarSchool)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage)]
    [Theory]
    public async Task Physics_WhenInvalidValues_ContainsNulls(MeasureSeriesType seriesType)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "x", prev: "y2", prev2: "3z")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, null, null, null));
    }

    [InlineData(MeasureSeriesType.CurrentSchool, 81.0, 80.0, 79.0)]
    [InlineData(MeasureSeriesType.SimilarSchool, 71.0, 70.0, 69.0)]
    [InlineData(MeasureSeriesType.EnglandSchoolsAverage, 61.0, 60.0, 59.0)]
    [Theory]
    public async Task Physics_ContainsYearByYearValues(MeasureSeriesType seriesType, double? current, double? prev, double? prev2)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x.WithPhysics49(current: "81", prev: "80", prev2: "79")),
            Build.Ks4Performance.Establishment("100002", x => x.WithPhysics49(current: "71", prev: "70", prev2: "69")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x.WithPhysics49(current: "61", prev: "60", prev2: "59")));

        var response = await _sut.Execute(Request("100001", "100002"));

        var series = response.Physics.Series
            .FirstOrDefault(s => s.SeriesType == seriesType);

        series.Should().NotBeNull();
        series.Should().Be(
            new MeasureSeries(seriesType, (decimal?)current, (decimal?)prev, (decimal?)prev2));
    }

    [InlineData(Ks4Physics.Filters.Grade.Values.Grade4AndAbove)]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade5AndAbove)]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade7AndAbove)]
    [Theory]
    public async Task Physics_FilterBy_Grade_WhenMissingEmptyOrInvalidValuesForSelectedSubject_ContainsNullValues(string subject)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithPhysics49(current: "x", prev: "y", prev2: "z")
                .WithPhysics59(current: "", prev: "", prev2: "")
                .WithPhysics79(current: "a", prev: "b", prev2: "c")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Physics.Filters.Grade.Key] = subject
        }));

        var series = response.Physics.Series;

        series.Should().NotBeNull();
        series.Should().Equal(
            new MeasureSeries(MeasureSeriesType.CurrentSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, null, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, null, null, null));
    }

    [InlineData(Ks4Physics.Filters.Grade.Values.Grade4AndAbove, new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade5AndAbove, new[] { 62.0, 61.0, 60.0 }, new[] { 61.0, 60.0, 59.0 }, new[] { 64.0, 63.0, 62.0 })]
    [InlineData(Ks4Physics.Filters.Grade.Values.Grade7AndAbove, new[] { 52.0, 51.0, 50.0 }, new[] { 51.0, 50.0, 49.0 }, new[] { 54.0, 53.0, 52.0 })]
    // Empty or invalid filter values default to Grade4AndAbove
    [InlineData("", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [InlineData("xyz", new[] { 72.0, 71.0, 70.0 }, new[] { 71.0, 70.0, 69.0 }, new[] { 74.0, 73.0, 72.0 })]
    [Theory]
    public async Task Physics_FilterBy_Grade_ContainsYearByYearValuesForSelectedSubject(string subject, double[] currentSchool, double[] similarSchools, double[] england)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Secondary()),
            Build.Establishment("100002", "Test School 2", x => x.Secondary()));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks4Performance.Establishment("100001", x => x
                .WithPhysics49(current: "72", prev: "71", prev2: "70")
                .WithPhysics59(current: "62", prev: "61", prev2: "60")
                .WithPhysics79(current: "52", prev: "51", prev2: "50")),
            Build.Ks4Performance.Establishment("100002", x => x
                .WithPhysics49(current: "71", prev: "70", prev2: "69")
                .WithPhysics59(current: "61", prev: "60", prev2: "59")
                .WithPhysics79(current: "51", prev: "50", prev2: "49")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks4Performance.England(x => x
                .WithPhysics49(current: "74", prev: "73", prev2: "72")
                .WithPhysics59(current: "64", prev: "63", prev2: "62")
                .WithPhysics79(current: "54", prev: "53", prev2: "52")));

        var response = await _sut.Execute(Request("100001", "100002", filterBy: new()
        {
            [Ks4Physics.Filters.Grade.Key] = subject
        }));

        var series = response.Physics.Series;

        series.Should().NotBeNull();
        series.Should().Equal([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, (decimal?)currentSchool[0], (decimal?)currentSchool[1], (decimal?)currentSchool[2]),
            new MeasureSeries(MeasureSeriesType.SimilarSchool, (decimal?)similarSchools[0], (decimal?)similarSchools[1], (decimal?)similarSchools[2]),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, (decimal?)england[0], (decimal?)england[1], (decimal?)england[2])
        ]);
    }

    private GetComparisonKs4CoreSubjectsMeasuresRequest Request(string currentSchoolUrn, string similarSchoolUrn, Dictionary<string, string>? filterBy = null) =>
            new(currentSchoolUrn, similarSchoolUrn, filterBy ?? []);
}
