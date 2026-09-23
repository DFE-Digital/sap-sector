using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.Data.Common.Tests.Catalogue;

public class DataMapEquivalenceTests
{
    private static DataMapRow Row(string property = "M_Tot_Est_Current_Num", string field = "f", string key = "URN") => new()
    {
        Range = "Establishment",
        Type = "T",
        PropertyName = property,
        FileName = "file",
        Field = field,
        DataType = "double",
        RecordFilterBy = key,
        Filter = "breakdown",
        FilterValue = "Total",
        Filter2 = "grade",
        Filter2Value = "9+8",
    };

    [Fact]
    public void Identical_rows_are_equivalent() =>
        DataMapEquivalence.Differences([Row()], [Row()]).Should().BeEmpty();

    [Fact]
    public void Ignores_filter_order_and_or_value_order()
    {
        var reordered = Row();
        (reordered.Filter, reordered.FilterValue, reordered.Filter2, reordered.Filter2Value) = ("grade", "8+9", "breakdown", "Total");

        DataMapEquivalence.Differences([Row()], [reordered]).Should().BeEmpty();
    }

    [Fact]
    public void Ignores_ignored_rows_and_later_duplicates_like_the_generator()
    {
        var ignored = Row("Other_Tot_Est_Current_Num");
        ignored.IgnoreMapping = "Y";

        DataMapEquivalence.Differences([Row(), Row(field: "different"), ignored], [Row()]).Should().BeEmpty();
    }

    [Fact]
    public void Compares_key_columns_after_generator_normalisation() =>
        DataMapEquivalence.Differences([Row(key: "URN")], [Row(key: "urn")]).Should().BeEmpty();

    [Fact]
    public void Reports_field_difference() =>
        DataMapEquivalence.Differences([Row()], [Row(field: "g")])
            .Should().ContainSingle().Which.Should().Contain("T/Establishment.M_Tot_Est_Current_Num");

    [Fact]
    public void Reports_key_difference() =>
        DataMapEquivalence.Differences([Row()], [Row(key: "school_urn")])
            .Should().ContainSingle().Which.Should().Contain("key 'urn' != 'school_urn'");

    [Fact]
    public void Reports_missing_and_unexpected_properties() =>
        DataMapEquivalence.Differences([Row("A_Tot_Est_Current_Num")], [Row("B_Tot_Est_Current_Num")])
            .Should().BeEquivalentTo(
                "T/Establishment.A_Tot_Est_Current_Num: missing property",
                "T/Establishment.B_Tot_Est_Current_Num: unexpected property");
}
