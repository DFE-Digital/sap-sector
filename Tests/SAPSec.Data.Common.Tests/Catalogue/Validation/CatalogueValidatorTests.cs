using SAPData.Models;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPSec.Data.Common.Tests.Catalogue.Validation;

public class CatalogueValidatorTests
{
    private static DataMapRow Row(
        string property = "M_Tot_Est_Current_Num",
        string yearDesc = "Current",
        string year = "2024-2025",
        string file = "schools",
        string field = "value",
        string key = "school_urn",
        string filter = "breakdown",
        string filterValue = "Total",
        string timePeriod = "202425") => new()
    {
        Range = "Establishment",
        Type = "T",
        PropertyName = property,
        Year = year,
        YearDesc = yearDesc,
        FileName = file,
        Field = field,
        DataType = "double",
        RecordFilterBy = key,
        Filter = filter,
        FilterValue = filterValue,
        Filter2 = "time_period",
        Filter2Value = timePeriod,
    };

    private static SourceProfiles Profiles() => new()
    {
        Files =
        {
            ["schools"] = new SourceProfile
            {
                Columns = new(["school_urn", "breakdown", "time_period", "value", "other_value"], StringComparer.Ordinal),
                Values =
                {
                    ["breakdown"] = new(["Total", "Boys"], StringComparer.Ordinal),
                    ["time_period"] = new(["202324", "202425"], StringComparer.Ordinal),
                },
            },
        },
    };

    [Fact]
    public void Valid_rows_have_no_issues()
    {
        var rows = new[] { Row(), Row("M_Boy_Est_Current_Num", filterValue: "Boys") };

        CatalogueValidator.Validate(rows, Profiles()).Should().BeEmpty();
    }

    [Fact]
    public void Ignored_and_unnamed_rows_are_skipped()
    {
        var ignored = Row(yearDesc: "Previous");
        ignored.IgnoreMapping = "Y";
        var unnamed = Row(property: "", yearDesc: "Previous");

        CatalogueValidator.Validate([ignored, unnamed]).Should().BeEmpty();
    }

    [Fact]
    public void Reports_a_name_whose_period_differs_from_the_row()
    {
        var issues = CatalogueValidator.Validate([Row("Prog8_Avg_LA_Previous2_Num", yearDesc: "Current")]);

        issues.Should().ContainSingle().Which.Should().Match<ValidationIssue>(i =>
            i.Rule == CatalogueValidator.PeriodInName && i.Property == "Prog8_Avg_LA_Previous2_Num");
    }

    [Fact]
    public void Reports_a_time_period_filter_for_a_different_year()
    {
        var issues = CatalogueValidator.Validate([Row(timePeriod: "202324")]);

        issues.Should().ContainSingle().Which.Rule.Should().Be(CatalogueValidator.TimePeriodFilter);
    }

    [Fact]
    public void Skips_the_time_period_check_for_non_academic_years() =>
        CatalogueValidator.Validate([Row("TrustsId", year: "Current", timePeriod: "anything")]).Should().BeEmpty();

    [Fact]
    public void Reports_properties_that_read_identical_data()
    {
        var issues = CatalogueValidator.Validate([Row("EngMaths49_Mob_Eng_Current_Num"), Row("EngMaths59_Mob_Eng_Current_Num")]);

        issues.Should().HaveCount(2).And.OnlyContain(i => i.Rule == CatalogueValidator.DuplicateMapping);
    }

    [Fact]
    public void Same_data_under_the_same_name_is_not_a_duplicate_mapping() =>
        CatalogueValidator.Validate([Row(), Row()]).Should().BeEmpty();

    [Fact]
    public void Reports_inconsistent_key_columns_for_a_file()
    {
        var issues = CatalogueValidator.Validate([Row(), Row("M_Boy_Est_Current_Num", filterValue: "Boys", key: "")]);

        issues.Should().ContainSingle().Which.Should().Match<ValidationIssue>(i =>
            i.Rule == CatalogueValidator.KeyColumn && i.Property == "schools");
    }

    [Fact]
    public void Key_columns_are_compared_after_normalisation() =>
        CatalogueValidator.Validate([Row(), Row("M_Boy_Est_Current_Num", filterValue: "Boys", key: "School_URN")]).Should().BeEmpty();

    [Fact]
    public void Reports_a_file_without_a_profile() =>
        CatalogueValidator.Validate([Row(file: "unknown")], Profiles())
            .Should().ContainSingle().Which.Rule.Should().Be(CatalogueValidator.UnknownFile);

    [Theory]
    [InlineData("missing_field", "school_urn", "breakdown")]
    [InlineData("value", "urn", "breakdown")]
    [InlineData("value", "school_urn", "sex")]
    public void Reports_fields_keys_and_filter_columns_missing_from_the_file(string field, string key, string filter)
    {
        var issues = CatalogueValidator.Validate([Row(field: field, key: key, filter: filter)], Profiles());

        issues.Should().ContainSingle().Which.Rule.Should().Be(CatalogueValidator.UnknownColumn);
    }

    [Fact]
    public void Reports_filter_values_that_match_no_rows()
    {
        var issues = CatalogueValidator.Validate([Row(filterValue: "7+8+Total")], Profiles());

        issues.Should().ContainSingle().Which.Should().Match<ValidationIssue>(i =>
            i.Rule == CatalogueValidator.UnknownValue && i.Message.Contains("'7', '8'"));
    }

    [Fact]
    public void Matches_source_columns_after_normalisation() =>
        CatalogueValidator.Validate([Row(field: "Other Value", key: "School URN")], Profiles()).Should().BeEmpty();
}
