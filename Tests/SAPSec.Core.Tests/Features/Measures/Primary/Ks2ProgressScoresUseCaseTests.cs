using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using static SAPSec.Core.Features.Measures.Measures.Primary;

namespace SAPSec.Core.Tests.Features.Measures.Primary;

public class Ks2ProgressScoresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryKs2PerformanceRepository _performanceRepo;
    private readonly GetSchoolKs2PerformanceMeasuresUseCase _school;
    private readonly GetComparisonKs2PerformanceMeasuresUseCase _comparison;

    public Ks2ProgressScoresUseCaseTests()
    {
        _performanceRepo = new(_establishmentRepo);
        _school = new(_establishmentRepo, _similarSchoolsRepo, _performanceRepo);
        _comparison = new(_establishmentRepo, _similarSchoolsRepo, _performanceRepo);

        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Primary().InLA("001")),
            Build.Establishment("100002", "Similar School A", x => x.Primary().InLA("001")),
            Build.Establishment("100003", "Similar School B", x => x.Primary().InLA("002")),
            Build.Establishment("100004", "Similar School C", x => x.Primary().InLA("002")));

        _similarSchoolsRepo.SetupGroups(Build.PrimaryGroup("100001", ["100002", "100003", "100004"]));

        _performanceRepo.SetupEstablishmentPerformance(
            Build.Ks2Performance.Establishment("100001", x => x
                .WithProgress("Reading", current: "-1.84", prev: "z", prev2: "0.3")
                .WithProgress("Writing", current: "2.1")
                .WithProgress("Maths", current: "SUPP")
                .WithEligiblePupils(current: "30", prev: "28", prev2: "58")
                .WithProgressIncluded("Reading", current: "", prev: "", prev2: "95%")
                .WithProgressIncluded("Maths", current: "", prev: "", prev2: "93%")),
            Build.Ks2Performance.Establishment("100002", x => x
                .WithProgress("Reading", current: "3.0")
                .WithEligiblePupils(current: "45")),
            Build.Ks2Performance.Establishment("100003", x => x
                .WithProgress("Reading", current: "-2.0")),
            Build.Ks2Performance.Establishment("100004", x => x
                .WithProgress("Reading", current: "c")));

        _performanceRepo.SetupLAPerformance(
            Build.Ks2Performance.LA("001", x => x.WithProgress("Reading", current: "-0.46", prev: "", prev2: "0.1")));

        _performanceRepo.SetupEnglandPerformance(
            Build.Ks2Performance.England(x => x.WithProgress("Reading", current: "0.04", prev: "0", prev2: "-0.02")));
    }

    private static Dictionary<string, string> Subject(string value) => new() { [Ks2Progress.Filters.Subject.Key] = value };

    [Fact]
    public async Task School_ProgressScores_IsAProgressScoreMeasureForTheLatestKs2Year()
    {
        var response = await _school.Execute(new("100001"));

        response.ProgressScores.Key.Should().Be("progress");
        response.ProgressScores.Name.Should().Be("Progress scores in reading, writing and maths");
        response.ProgressScores.DataType.Should().Be(MeasureDataType.ProgressScore);
        response.ProgressScores.Year.Should().Be(2024);
    }

    [Fact]
    public async Task School_ProgressScores_SubjectFilterDefaultsToReadingWithOptionsInOrder()
    {
        var response = await _school.Execute(new("100001"));

        var filter = response.ProgressScores.Filters.Should().ContainSingle().Subject;
        filter.Key.Should().Be("progress-subject");
        filter.Name.Should().Be("Subject");
        filter.Options.Select(o => o.Name).Should().Equal("Reading", "Writing", "Maths");
        filter.Options.Single(o => o.Selected).Name.Should().Be("Reading");
    }

    [Fact]
    public async Task School_ProgressScores_Reading_SeriesForSchoolSimilarSchoolsLaAndEngland()
    {
        var response = await _school.Execute(new("100001"));

        response.ProgressScores.Series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, -1.84m, null, 0.3m),
            // Mean of the similar schools' values that are numbers (3.0 and -2.0), to 1dp.
            new MeasureSeries(MeasureSeriesType.SimilarSchoolsAverage, 0.5m, null, null),
            new MeasureSeries(MeasureSeriesType.LASchoolsAverage, -0.46m, null, 0.1m),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 0.04m, 0m, -0.02m)
        ], o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task School_ProgressScores_WritingFilter_UsesWritingScores()
    {
        var response = await _school.Execute(new("100001", Subject(Ks2Progress.Filters.Subject.Values.Writing)));

        response.ProgressScores.Series.First().Current.Should().Be(2.1m);
        response.ProgressScores.Filters.Single().Options.Single(o => o.Selected).Name.Should().Be("Writing");
    }

    [Fact]
    public async Task School_ProgressScores_SuppressedValue_IsMissing()
    {
        var response = await _school.Execute(new("100001", Subject(Ks2Progress.Filters.Subject.Values.Maths)));

        response.ProgressScores.Series.First().Current.Should().BeNull();
    }

    [Fact]
    public async Task School_ProgressScores_InvalidSubject_FallsBackToReading()
    {
        var response = await _school.Execute(new("100001", Subject("not-a-subject")));

        response.ProgressScores.Series.First().Current.Should().Be(-1.84m);
        response.ProgressScores.Filters.Single().Options.Single(o => o.Selected).Name.Should().Be("Reading");
    }

    [Fact]
    public async Task School_ProgressScores_TopPerformersRankedOnLatestYearHighestFirst()
    {
        var response = await _school.Execute(new("100001"));

        // The user's school is ranked with its similar schools; 'c' (suppressed) is left out.
        response.ProgressScores.TopPerformers.Should().BeEquivalentTo([
            new TopPerformer(1, "100002", "Similar School A", 3.0m),
            new TopPerformer(2, "100001", "Current School", -1.84m, IsCurrentSchool: true),
            new TopPerformer(3, "100003", "Similar School B", -2.0m)
        ], o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task School_ProgressPupilNumbers_ForTheSchoolAndEverySimilarSchool()
    {
        var response = await _school.Execute(new("100001"));

        response.ProgressPupilNumbers.Keys.Should().BeEquivalentTo(["100001", "100002", "100003", "100004"]);

        var current = response.ProgressPupilNumbers["100001"];
        current.EligiblePupils.Should().Be(new PupilNumbersYears<int>(58, 28, 30));
        current.ProportionIncluded.Should().Be(new PupilNumbersYears<decimal>(95m, null, null));

        response.ProgressPupilNumbers["100002"].EligiblePupils.Current.Should().Be(45);
    }

    [Fact]
    public async Task School_ProgressPupilNumbers_ProportionFollowsTheSubjectFilter()
    {
        var response = await _school.Execute(new("100001", Subject(Ks2Progress.Filters.Subject.Values.Maths)));

        var current = response.ProgressPupilNumbers["100001"];
        current.ProportionIncluded.Should().Be(new PupilNumbersYears<decimal>(93m, null, null));
        current.EligiblePupils.Should().Be(new PupilNumbersYears<int>(58, 28, 30));
    }

    [Fact]
    public async Task Comparison_ProgressScores_SeriesForBothSchoolsAndEngland()
    {
        var response = await _comparison.Execute(new("100001", "100002"));

        response.ProgressScores.DataType.Should().Be(MeasureDataType.ProgressScore);
        response.ProgressScores.TopPerformers.Should().BeNull();
        response.ProgressScores.Series.Should().BeEquivalentTo([
            new MeasureSeries(MeasureSeriesType.CurrentSchool, -1.84m, null, 0.3m),
            new MeasureSeries(MeasureSeriesType.ComparatorSchool, 3.0m, null, null),
            new MeasureSeries(MeasureSeriesType.EnglandSchoolsAverage, 0.04m, 0m, -0.02m)
        ], o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task Comparison_ProgressPupilNumbers_ForBothSchools()
    {
        var response = await _comparison.Execute(new("100001", "100002"));

        response.CurrentSchoolProgressPupilNumbers.EligiblePupils.Current.Should().Be(30);
        response.ComparatorSchoolProgressPupilNumbers.Urn.Should().Be("100002");
        response.ComparatorSchoolProgressPupilNumbers.EligiblePupils.Current.Should().Be(45);
    }
}
