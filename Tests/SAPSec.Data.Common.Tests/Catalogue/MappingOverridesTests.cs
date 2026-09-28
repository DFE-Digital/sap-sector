using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.Data.Common.Tests.Catalogue;

public class MappingOverridesTests
{
    private static IReadOnlyList<DataMapRow> CodeRows() =>
        new MeasureSet("KS4_Performance", "Performance")
            .Years(2024)
            .Breakdowns(Breakdowns.Total, Breakdowns.Boys)
            .Source(Scope.Establishment, Period.Current, Source.Ees("schools_2024")
                .KeyedBy("school_urn")
                .Where("time_period", "202425")
                .Provides(Breakdowns.Total, ("breakdown", "Total"))
                .Provides(Breakdowns.Boys, ("breakdown", "Boys")))
            .Metric(new Metric("Attainment8", "attainment8_average").In(Scope.Establishment).During(Period.Current))
            .ToDataMapRows();

    // The mapping list as import-map reads it: the definitions' mappings, some edited.
    private static List<Mapping> List(IReadOnlyList<DataMapRow> rows) => rows.Select(Mapping.FromRow).ToList();

    [Fact]
    public void An_unedited_list_has_no_overrides()
    {
        var code = CodeRows();

        MappingOverrides.FromEditedList(code, List(code)).Mappings.Should().BeEmpty();
    }

    [Fact]
    public void A_changed_line_overrides_that_mapping_only()
    {
        var code = CodeRows();
        var list = List(code);
        var boys = list.FindIndex(m => m.Property == "Attainment8_Boy_Est_Current_Num");
        list[boys] = list[boys] with { ValueColumn = "attainment8_boys", Filters = new() { ["breakdown"] = ["Male"], ["time_period"] = ["202425"] } };

        var overrides = MappingOverrides.FromEditedList(code, list);
        var rows = overrides.ApplyTo(code);

        overrides.Mappings.Should().ContainSingle().Which.Property.Should().Be("Attainment8_Boy_Est_Current_Num");
        rows.Should().HaveSameCount(code);
        var row = rows.Single(r => r.PropertyName == "Attainment8_Boy_Est_Current_Num");
        row.Field.Should().Be("attainment8_boys");
        (row.Filter, row.FilterValue, row.Filter2, row.Filter2Value).Should().Be(("breakdown", "Male", "time_period", "202425"));
        row.PropertyDescription.Should().Be(code.Single(r => r.PropertyName == row.PropertyName).PropertyDescription);
        rows.Single(r => r.PropertyName == "Attainment8_Tot_Est_Current_Num").Should().BeSameAs(code.Single(r => r.PropertyName == "Attainment8_Tot_Est_Current_Num"));
    }

    [Fact]
    public void A_new_line_adds_a_mapping()
    {
        var code = CodeRows();
        var list = List(code);
        list.Add(list[0] with { Property = "Attainment8_Girls_Est_Current_Num", Filters = new() { ["breakdown"] = ["Girls"] } });

        var rows = MappingOverrides.FromEditedList(code, list).ApplyTo(code);

        rows.Should().HaveCount(code.Count + 1);
        rows[^1].PropertyName.Should().Be("Attainment8_Girls_Est_Current_Num");
        (rows[^1].Filter, rows[^1].FilterValue).Should().Be(("breakdown", "Girls"));
    }

    [Fact]
    public void A_line_changed_back_is_no_longer_an_override()
    {
        var code = CodeRows();
        var list = List(code);
        list[0] = list[0] with { Overridden = true };

        MappingOverrides.FromEditedList(code, list).Mappings.Should().BeEmpty();
    }

    [Fact]
    public void Removed_lines_are_rejected()
    {
        var code = CodeRows();

        var act = () => MappingOverrides.FromEditedList(code, List(code).Skip(1).ToList());

        act.Should().Throw<CatalogueException>().WithMessage("*removed or renamed*Attainment8_*");
    }

    [Fact]
    public void Duplicate_lines_are_rejected()
    {
        var code = CodeRows();
        var list = List(code);
        list.Add(list[0]);

        var act = () => MappingOverrides.FromEditedList(code, list);

        act.Should().Throw<CatalogueException>().WithMessage("*appears 2 times*");
    }

    [Fact]
    public void Invalid_lines_are_rejected()
    {
        var code = CodeRows();
        var list = List(code);
        list[0] = list[0] with { ValueColumn = "", Filters = new() { ["breakdown"] = ["Boys+Girls"] } };

        var act = () => MappingOverrides.FromEditedList(code, list);

        act.Should().Throw<CatalogueException>().WithMessage("*'valueColumn' must not be empty*");
    }

    [Fact]
    public void Overrides_round_trip_through_their_file()
    {
        var code = CodeRows();
        var list = List(code);
        list[0] = list[0] with { ValueColumn = "other" };
        var overrides = MappingOverrides.FromEditedList(code, list);

        var reloaded = MappingOverrides.Parse(overrides.ToJson());

        reloaded.Mappings.Select(m => m.ToLine()).Should().Equal(overrides.Mappings.Select(m => m.ToLine()));
    }

    [Fact]
    public void The_list_marks_overridden_mappings_and_reads_back()
    {
        var code = CodeRows();
        var list = List(code);
        list[0] = list[0] with { ValueColumn = "other" };
        var overrides = MappingOverrides.FromEditedList(code, list);

        var (version, mappings) = DataMapExport.Parse(DataMapExport.ToJson(code, overrides));

        version.Should().Be(DataMapExport.CodeVersion(code));
        mappings.Single(m => m.Property == list[0].Property).Overridden.Should().BeTrue();
        mappings.Where(m => m.Property != list[0].Property).Should().OnlyContain(m => m.Overridden == null);
    }

    [Fact]
    public void The_code_version_changes_when_the_definitions_do()
    {
        var code = CodeRows();
        var changed = code.Select((r, i) => i == 0 ? Mapping.FromRow(r) with { ValueColumn = "other" } : Mapping.FromRow(r))
            .Select(m => m.ToRow())
            .ToList();

        DataMapExport.CodeVersion(changed).Should().NotBe(DataMapExport.CodeVersion(code));
    }
}
