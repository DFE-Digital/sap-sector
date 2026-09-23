using SAPSec.Data.Common.Catalogue;

namespace SAPSec.Data.Common.Tests.Catalogue;

/// <summary>
/// Reproduces real datamap.csv rows for Attainment 8 (Establishment, Boys) across all three years,
/// covering a different source layout per year.
/// </summary>
public class Attainment8ExampleTests
{
    private static Source PerformanceTables(string file, AcademicYear year) =>
        Source.Ees(file)
            .KeyedBy("school_urn")
            .Where("time_period", year.Code)
            .Provides(Breakdowns.Total, ("breakdown", "Total"))
            .Provides(Breakdowns.Boys, ("breakdown", "Boys"));

    private static readonly MeasureSet Ks4Performance = CreateKs4Performance();

    private static MeasureSet CreateKs4Performance()
    {
        var current = PerformanceTables("202425_performance_tables_schools_final", new AcademicYear(2024));
        var previous = PerformanceTables("202324_performance_tables_schools_final", new AcademicYear(2023));
        var previous2 = Source.Cscp("2022-2023_england_ks4final").KeyedBy("URN");

        return new MeasureSet("KS4_Performance", "Performance")
            .Years(2024)
            .Breakdowns(Breakdowns.Standard)
            .Source(Scope.Establishment, Period.Current, current)
            .Source(Scope.Establishment, Period.Previous, previous)
            .Source(Scope.Establishment, Period.Previous2, previous2)
            .Metric("Attainment8", "attainment8_average", m => m
                .Field(previous, "avg_att8")
                .Field(previous2, Breakdowns.Total, "ATT8SCR")
                .Field(previous2, Breakdowns.Boys, "ATT8SCR_BOYS"));
    }

    [Theory]
    [InlineData("Attainment8_Boy_Est_Current_Num", "2024-2025", "202425_performance_tables_schools_final", "attainment8_average", "school_urn", "breakdown", "Boys", "time_period", "202425")]
    [InlineData("Attainment8_Boy_Est_Previous_Num", "2023-2024", "202324_performance_tables_schools_final", "avg_att8", "school_urn", "breakdown", "Boys", "time_period", "202324")]
    [InlineData("Attainment8_Boy_Est_Previous2_Num", "2022-2023", "2022-2023_england_ks4final", "ATT8SCR_BOYS", "URN", "", "", "", "")]
    public void Matches_datamap_row(
        string propertyName, string year, string file, string field, string key,
        string filter, string filterValue, string filter2, string filter2Value)
    {
        var row = Ks4Performance.ToDataMapRows().Single(r => r.PropertyName == propertyName);

        row.Range.Should().Be("Establishment");
        row.Type.Should().Be("KS4_Performance");
        row.Year.Should().Be(year);
        row.FileName.Should().Be(file);
        row.Field.Should().Be(field);
        row.DataType.Should().Be("double");
        row.RecordFilterBy.Should().Be(key);
        (row.Filter, row.FilterValue, row.Filter2, row.Filter2Value).Should().Be((filter, filterValue, filter2, filter2Value));
    }

    [Fact]
    public void Expands_to_one_row_per_year_and_provided_breakdown()
    {
        Ks4Performance.ToDataMapRows().Should().HaveCount(6);
    }
}
