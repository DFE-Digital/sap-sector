using SAPSec.Data.Common.Catalogue;

namespace SAPSec.Data.Common.Tests.Catalogue;

public class CatalogueTypesTests
{
    [Theory]
    [InlineData(2024, "2024-2025", "202425")]
    [InlineData(2099, "2099-2100", "209900")]
    public void AcademicYear_formats_label_and_code(int startYear, string label, string code)
    {
        var year = new AcademicYear(startYear);

        year.Label.Should().Be(label);
        year.Code.Should().Be(code);
    }

    [Fact]
    public void Filter_rejects_plus_in_value()
    {
        var act = () => new Filter("grade", "9+8");

        act.Should().Throw<CatalogueException>().WithMessage("*must not contain '+'*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Filter_rejects_empty_value(string value)
    {
        var act = () => new Filter("grade", value);

        act.Should().Throw<CatalogueException>();
    }

    [Fact]
    public void Filter_requires_a_value()
    {
        var act = () => new Filter("grade");

        act.Should().Throw<CatalogueException>();
    }

    [Theory]
    [InlineData(DataType.Double, "double")]
    [InlineData(DataType.String, "string")]
    [InlineData(DataType.StringArray, "string array")]
    public void DataType_maps_to_datamap_value(DataType dataType, string expected) =>
        dataType.DataMapValue().Should().Be(expected);

    [Theory]
    [InlineData(Scope.Establishment, "Est")]
    [InlineData(Scope.LA, "LA")]
    [InlineData(Scope.England, "Eng")]
    public void Scope_maps_to_name_code(Scope scope, string expected) =>
        scope.NameCode().Should().Be(expected);
}
