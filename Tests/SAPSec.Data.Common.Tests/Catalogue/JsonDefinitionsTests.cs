using SAPData.Models;
using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;

namespace SAPSec.Data.Common.Tests.Catalogue;

public class JsonDefinitionsTests
{
    // DataYears.Ks4Performance is the current year for every test dataset below.
    private static readonly string Current = new AcademicYear(SAPSec.Data.DataYears.Ks4Performance).Code;

    private static List<DataMapRow> Rows(string json) =>
        DataMapCatalogue.Expand(JsonDefinitions.LoadDataset("test.json", json)).ToList();

    private static string Dataset(string sources, string measures, string extra = "") => $$"""
        {
          "dataset": "T",
          {{extra}}
          "measureSets": [
            {
              "subtype": "S",
              "yearsFrom": "Ks4Performance",
              "periods": ["Current"],
              "pupilGroups": ["Total", "Boys"],
              "sources": [{{sources}}],
              "measures": [{{measures}}]
            }
          ]
        }
        """;

    private const string Schools = """
        { "id": "schools", "scopes": ["Establishment"], "publisher": "EES", "file": "schools_{year}", "key": "school_urn",
          "where": { "time_period": "{year}" },
          "pupilGroups": { "Total": { "breakdown": "Total" }, "Boys": { "breakdown": "Boys" } } }
        """;

    [Fact]
    public void A_measure_reads_its_column_from_each_pupil_group_row()
    {
        var rows = Rows(Dataset(Schools, """{ "name": "Attainment8", "column": "attainment8_average" }"""));

        rows.Select(r => r.PropertyName).Should().Equal("Attainment8_Tot_Est_Current_Num", "Attainment8_Boy_Est_Current_Num");
        var boys = rows[1];
        (boys.FileName, boys.RecordFilterBy, boys.Field).Should().Be(($"schools_{Current}", "school_urn", "attainment8_average"));
        (boys.Filter, boys.FilterValue, boys.Filter2, boys.Filter2Value).Should().Be(("breakdown", "Boys", "time_period", Current));
    }

    [Fact]
    public void ForEach_repeats_measures_for_every_combination()
    {
        var rows = Rows(Dataset(Schools,
            """
            { "forEach": { "subject": "subjects", "band": ["4", "5"] },
              "measures": [{ "name": "{subject.code}{band}", "column": "value", "pupilGroups": ["Total"],
                             "where": { "subject": "{subject.name}", "grade": "{band}" } }] }
            """,
            """ "lists": { "subjects": [{ "code": "Bio", "name": "Biology" }, { "code": "Chem", "name": "Chemistry" }] }, """));

        rows.Select(r => r.PropertyName).Should().Equal(
            "Bio4_Tot_Est_Current_Num", "Bio5_Tot_Est_Current_Num", "Chem4_Tot_Est_Current_Num", "Chem5_Tot_Est_Current_Num");
        (rows[2].Filter2, rows[2].Filter2Value, rows[2].Filter3, rows[2].Filter3Value).Should().Be(("subject", "Chemistry", "grade", "4"));
    }

    [Fact]
    public void Keys_and_filters_can_differ_by_scope()
    {
        var rows = Rows(Dataset(
            """
            { "scopes": ["England", "LA"], "publisher": "EES", "file": "areas", "key": { "England": "geographic_level", "LA": "old_la_code" },
              "where": { "geographic_level": { "England": "National" }, "time_period": "{year}" },
              "pupilGroups": "all" }
            """,
            """{ "name": "M", "column": "value" }""",
            """ "pupilGroupSets": { "all": { "Total": {} } }, """));

        var england = rows.Single(r => r.Range == "England");
        var la = rows.Single(r => r.Range == "LA");
        (england.RecordFilterBy, england.Filter, england.FilterValue).Should().Be(("geographic_level", "geographic_level", "National"));
        (la.RecordFilterBy, la.Filter, la.FilterValue).Should().Be(("old_la_code", "time_period", Current));
    }

    [Fact]
    public void A_wide_file_names_a_column_per_pupil_group()
    {
        var rows = Rows(Dataset(
            """{ "id": "wide", "scopes": ["Establishment"], "publisher": "CSCP", "file": "wide", "key": "URN" }""",
            """
            { "name": "Abs", "unit": "Pct", "nameTemplate": "{metric}{_breakdown}_{scope}_{period}_Pct",
              "columnBySource": { "wide": { "Total": "all_pct", "Boys": "boy_pct" } } }
            """));

        rows.Select(r => (r.PropertyName, r.Field)).Should().Equal(("Abs_Est_Current_Pct", "all_pct"), ("Abs_Boy_Est_Current_Pct", "boy_pct"));
    }

    [Theory]
    [InlineData("""{ "name": "M", "colum": "value" }""", "test.json: $.measureSets[0].measures[0] (M): unknown setting(s) 'colum'*")]
    [InlineData("""{ "name": "M", "column": "value", "pupilGroups": ["Boyz"] }""", "*test.json*unknown pupil group 'Boyz'*")]
    [InlineData("""{ "name": "M", "column": "value", "columnBySource": { "school": "x" } }""", "*test.json*no source with \"id\": \"school\"*")]
    [InlineData("""{ "name": "M", "column": "value", "unit": "Percent" }""", "*test.json*'Percent' should be Num or Pct*")]
    [InlineData("""{ "forEach": { "s": "missing" }, "measures": [{ "name": "{s}" }] }""", "*test.json*expected a list, or the name of one*")]
    public void Mistakes_are_reported_with_the_file_and_setting(string measure, string message)
    {
        var act = () => Rows(Dataset(Schools, measure));

        act.Should().Throw<CatalogueException>().WithMessage(message);
    }

    [Fact]
    public void A_year_not_in_DataYears_is_reported() =>
        FluentActions.Invoking(() => Rows(Dataset(Schools, """{ "name": "M", "column": "v" }""").Replace("\"Ks4Performance\"", "\"Ks9\"")))
            .Should().Throw<CatalogueException>().WithMessage("*'Ks9' isn't a year in Data/SAPSec.Data/DataYears.cs*");
}
