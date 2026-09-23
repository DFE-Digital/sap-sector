using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.Data.Common.Tests.Catalogue;

public class MeasureSetTests
{
    private static Source SchoolSource(string file = "schools_2024", string timePeriod = "202425") =>
        Source.Ees(file)
            .KeyedBy("school_urn")
            .Where("time_period", timePeriod)
            .Provides(Breakdowns.Total, ("breakdown", "Total"))
            .Provides(Breakdowns.Boys, ("breakdown", "Boys"));

    [Fact]
    public void Expands_each_metric_across_scopes_periods_and_breakdowns()
    {
        var set = new MeasureSet("KS4_Performance", "Performance")
            .Years(2024)
            .Breakdowns(Breakdowns.Total, Breakdowns.Boys)
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Source(Scope.Establishment, Period.Previous, SchoolSource("schools_2023", "202324"))
            .Source(Scope.LA, Period.Current, SchoolSource("la_2024").KeyedBy("old_la_code"))
            .Source(Scope.LA, Period.Previous, SchoolSource("la_2023", "202324").KeyedBy("old_la_code"))
            .Metric("Attainment8", "attainment8_average", m => m.During(Period.Current, Period.Previous));

        var rows = set.ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal(
            "Attainment8_Tot_Est_Current_Num",
            "Attainment8_Boy_Est_Current_Num",
            "Attainment8_Tot_Est_Previous_Num",
            "Attainment8_Boy_Est_Previous_Num",
            "Attainment8_Tot_LA_Current_Num",
            "Attainment8_Boy_LA_Current_Num",
            "Attainment8_Tot_LA_Previous_Num",
            "Attainment8_Boy_LA_Previous_Num");
    }

    [Fact]
    public void Populates_row_from_metric_source_and_year()
    {
        var row = new MeasureSet("KS4_Performance", "Performance")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("Attainment8", "attainment8_average", m => m.Described("Attainment 8"))
            .ToDataMapRows()
            .Single();

        row.Should().BeEquivalentTo(new DataMapRow
        {
            Range = "Establishment",
            Ref = "Attainment8_Tot_Est_Current_Num",
            PropertyName = "Attainment8_Tot_Est_Current_Num",
            PropertyDescription = "Attainment 8 (Total)",
            Source = "EES",
            Type = "KS4_Performance",
            Subtype = "Performance",
            Year = "2024-2025",
            YearDesc = "Current",
            FileName = "schools_2024",
            Field = "attainment8_average",
            DataType = "double",
            RecordFilterBy = "school_urn",
            Filter = "breakdown",
            FilterValue = "Total",
            Filter2 = "time_period",
            Filter2Value = "202425",
        });
    }

    [Fact]
    public void Orders_filters_breakdown_then_metric_then_source()
    {
        var source = Source.Ees("subjects")
            .KeyedBy("school_urn")
            .Where("time_period", "202425")
            .Provides(Breakdowns.Boys, ("breakdown_topic", "Sex"), ("sex", "Boys"));

        var row = new MeasureSet("KS4_Performance", "SubjectEntries")
            .Year(Period.Current, new AcademicYear(2024))
            .Breakdowns(Breakdowns.Boys)
            .Source(Scope.Establishment, Period.Current, source)
            .Metric("Bio49", "number_achieving", m => m.Where("subject", "Biology").Where("grade", "9 to 4"))
            .ToDataMapRows()
            .Single();

        (row.Filter, row.FilterValue).Should().Be(("breakdown_topic", "Sex"));
        (row.Filter2, row.Filter2Value).Should().Be(("sex", "Boys"));
        (row.Filter3, row.Filter3Value).Should().Be(("subject", "Biology"));
        (row.Filter4, row.Filter4Value).Should().Be(("grade", "9 to 4"));
        (row.Filter5, row.Filter5Value).Should().Be(("time_period", "202425"));
        row.Filter6.Should().BeEmpty();
    }

    [Fact]
    public void Encodes_multiple_filter_values_as_or()
    {
        var row = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.England, Period.Current, Source.Ees("f").KeyedBy("geographic_level").Provides(Breakdowns.Total))
            .Metric("M", "field", m => m.Where("grade", "9", "8", "7"))
            .ToDataMapRows()
            .Single();

        row.FilterValue.Should().Be("9+8+7");
    }

    [Fact]
    public void Metric_can_override_field_per_source_and_per_breakdown()
    {
        var current = SchoolSource();
        var previous = SchoolSource("schools_2023", "202324");

        // Mirrors the 2022-23 CSCP file, where each breakdown is its own column.
        var cscp = Source.Cscp("2022-2023_england_ks4final").KeyedBy("URN");

        var rows = new MeasureSet("KS4_Performance", "Performance")
            .Years(2024)
            .Breakdowns(Breakdowns.Total, Breakdowns.Boys)
            .Source(Scope.Establishment, Period.Current, current)
            .Source(Scope.Establishment, Period.Previous, previous)
            .Source(Scope.Establishment, Period.Previous2, cscp)
            .Metric("Attainment8", "attainment8_average", m => m
                .Field(previous, "avg_att8")
                .Field(cscp, Breakdowns.Total, "ATT8SCR")
                .Field(cscp, Breakdowns.Boys, "ATT8SCR_BOYS"))
            .ToDataMapRows();

        rows.Select(r => (r.PropertyName, r.Field, r.Filter)).Should().Equal(
            ("Attainment8_Tot_Est_Current_Num", "attainment8_average", "breakdown"),
            ("Attainment8_Boy_Est_Current_Num", "attainment8_average", "breakdown"),
            ("Attainment8_Tot_Est_Previous_Num", "avg_att8", "breakdown"),
            ("Attainment8_Boy_Est_Previous_Num", "avg_att8", "breakdown"),
            ("Attainment8_Tot_Est_Previous2_Num", "ATT8SCR", ""),
            ("Attainment8_Boy_Est_Previous2_Num", "ATT8SCR_BOYS", ""));
    }

    [Fact]
    public void Field_based_source_only_yields_breakdowns_with_a_field()
    {
        var cscp = Source.Cscp("ks4final").KeyedBy("URN");

        var rows = new MeasureSet("T", "S")
            .Year(Period.Previous2, new AcademicYear(2022))
            .Breakdowns(Breakdowns.Standard)
            .Source(Scope.Establishment, Period.Previous2, cscp)
            .Metric("EngMaths49", "unused", m => m.Field(cscp, Breakdowns.Total, "PTL2BASICS_94"))
            .Metric("Other", "unused")
            .ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal("EngMaths49_Tot_Est_Previous2_Num");
    }

    [Fact]
    public void Metric_can_add_filters_for_one_source()
    {
        var current = SchoolSource();
        var older = Source.From("DfE", "underlying").KeyedBy("URN").Provides(Breakdowns.Total);

        var rows = new MeasureSet("T", "S")
            .Years(2024)
            .Source(Scope.Establishment, Period.Current, current)
            .Source(Scope.Establishment, Period.Previous, older)
            .Metric("Bio4", "number_achieving", m => m
                .During(Period.Current, Period.Previous)
                .Where(current, "subject", "Biology")
                .Where(older, "Discount Code", "RH3"))
            .ToDataMapRows();

        (rows[0].Filter2, rows[0].Filter2Value).Should().Be(("subject", "Biology"));
        (rows[1].Filter, rows[1].FilterValue).Should().Be(("Discount Code", "RH3"));
    }

    [Fact]
    public void Metric_can_skip_a_scope_and_period()
    {
        var rows = new MeasureSet("T", "S")
            .Years(2024)
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Source(Scope.Establishment, Period.Previous, SchoolSource("schools_2023", "202324"))
            .Metric("M", "field", m => m.During(Period.Current, Period.Previous).Skip(Scope.Establishment, Period.Previous))
            .ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal("M_Tot_Est_Current_Num");
    }

    [Fact]
    public void Skips_breakdowns_the_source_does_not_provide()
    {
        var rows = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Breakdowns(Breakdowns.Standard)
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("M", "field")
            .ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal("M_Tot_Est_Current_Num", "M_Boy_Est_Current_Num");
    }

    [Fact]
    public void Metric_can_restrict_scopes_breakdowns_and_use_a_name_template()
    {
        var rows = new MeasureSet("KS2_Performance", "Performance")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Source(Scope.LA, Period.Current, SchoolSource("la").KeyedBy("old_la_code"))
            .Metric("RwmExpected", "expected_standard_pupil_percent", m => m
                .In(Scope.Establishment)
                .For(Breakdowns.Boys)
                .Percentage()
                .Named("{metric}_Reading_{breakdown}_Cohort_{scope}_{period}_{unit}"))
            .ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal("RwmExpected_Reading_Boy_Cohort_Est_Current_Pct");
    }

    [Fact]
    public void Metric_can_name_properties_with_a_function()
    {
        var rows = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Breakdowns(Breakdowns.Total, Breakdowns.Boys)
            .Source(Scope.LA, Period.Current, SchoolSource("la").KeyedBy("old_la_code"))
            .Metric("Abs", "value", m => m.Named((b, s, p) =>
                b == Breakdowns.Total ? $"Abs_Tot_{s.NameCode()}_{p}" : $"Abs_Tot_{b.Code}_{s.NameCode()}_{p}"))
            .ToDataMapRows();

        rows.Select(r => r.PropertyName).Should().Equal("Abs_Tot_LA_Current", "Abs_Tot_Boy_LA_Current");
    }

    [Fact]
    public void Appends_raw_rows_after_expanded_metrics()
    {
        var raw = new DataMapRow { Range = "Establishment", Type = "All establishment data", PropertyName = "TrustsId" };

        var rows = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("M", "field", m => m.For(Breakdowns.Total))
            .Row(raw)
            .ToDataMapRows();

        rows.Should().HaveCount(2);
        rows[1].Should().BeSameAs(raw);
    }

    [Fact]
    public void Throws_when_a_metric_has_no_source_for_a_scope_and_period()
    {
        var set = new MeasureSet("KS4_Performance", "Performance")
            .Years(2024)
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("Attainment8", "attainment8_average");

        var act = () => set.ToDataMapRows();

        act.Should().Throw<CatalogueException>()
            .WithMessage("*'Attainment8' has no source for Establishment/Previous*");
    }

    [Fact]
    public void Throws_when_a_metric_uses_a_period_without_a_year()
    {
        var set = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("M", "field", m => m.During(Period.Previous));

        var act = () => set.ToDataMapRows();

        act.Should().Throw<CatalogueException>().WithMessage("*period Previous*no year*");
    }

    [Fact]
    public void Throws_when_a_source_has_no_key_column()
    {
        var set = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, Source.Ees("f").Provides(Breakdowns.Total))
            .Metric("M", "field");

        var act = () => set.ToDataMapRows();

        act.Should().Throw<CatalogueException>().WithMessage("*'f'*no key column*");
    }

    [Fact]
    public void Throws_on_duplicate_property_names()
    {
        var set = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.Establishment, Period.Current, SchoolSource())
            .Metric("M", "field_a", m => m.For(Breakdowns.Total))
            .Metric("M", "field_b", m => m.For(Breakdowns.Total));

        var act = () => set.ToDataMapRows();

        act.Should().Throw<CatalogueException>().WithMessage("*M_Tot_Est_Current_Num (2x)*");
    }

    [Fact]
    public void Throws_when_more_filters_than_the_datamap_supports()
    {
        var source = Source.Ees("f").KeyedBy("k").Provides(Breakdowns.Total);
        for (var i = 0; i < 10; i++)
            source.Where($"c{i}", "v");

        var set = new MeasureSet("T", "S")
            .Year(Period.Current, new AcademicYear(2024))
            .Source(Scope.England, Period.Current, source)
            .Metric("M", "field");

        var act = () => set.ToDataMapRows();

        act.Should().Throw<CatalogueException>().WithMessage("*10 filters*at most 9*");
    }
}
