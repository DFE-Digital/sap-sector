using FluentAssertions;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Tests.ViewModels.Measures;

public class NumberOfPupilsViewModelTests
{
    private static SchoolPupilNumbers Numbers(PupilNumbersYears<decimal>? proportion) =>
        new("100001", new PupilNumbersYears<int>(55, 1234, 60), proportion);

    [Fact]
    public void ForLatestYear_HasOneYearColumnForTheLatestYear()
    {
        var result = NumberOfPupilsViewModel.ForLatestYear("id", 2024,
            [("School A", Numbers(new PupilNumbersYears<decimal>(91m, 92m, 93m)))]);

        result.YearLabels.Should().Equal("2024 to 2025");
        result.Schools.Single().EligiblePupils.Should().Equal("60");
        result.Schools.Single().ProportionIncluded.Should().Equal("93%");
    }

    [Fact]
    public void ForYearByYear_HasThreeYearColumnsOldestFirst()
    {
        var result = NumberOfPupilsViewModel.ForYearByYear("id", 2024,
            [("School A", Numbers(new PupilNumbersYears<decimal>(91m, 92m, 93m)))]);

        result.YearLabels.Should().Equal("2022 to 2023", "2023 to 2024", "2024 to 2025");
        result.Schools.Single().EligiblePupils.Should().Equal("55", "1,234", "60");
        result.Schools.Single().ProportionIncluded.Should().Equal("91%", "92%", "93%");
    }

    [Theory]
    [InlineData(95.5, "96%")]
    [InlineData(95.49, "95%")]
    [InlineData(100, "100%")]
    [InlineData(0, "0%")]
    public void Proportion_IsAPercentageToZeroDecimalPlaces(decimal value, string expected)
    {
        var result = NumberOfPupilsViewModel.ForLatestYear("id", 2024,
            [("School A", Numbers(new PupilNumbersYears<decimal>(null, null, value)))]);

        result.Schools.Single().ProportionIncluded.Should().Equal(expected);
    }

    [Fact]
    public void Proportion_NotPublishedForPupilCharacteristic_ShowsNotApplicable()
    {
        var result = NumberOfPupilsViewModel.ForYearByYear("id", 2024, [("School A", Numbers(null))]);

        result.Schools.Single().ProportionIncluded.Should().Equal("N/A", "N/A", "N/A");
        result.Schools.Single().EligiblePupils.Should().Equal("55", "1,234", "60");
    }

    [Fact]
    public void MissingValues_ShowNoAvailableData()
    {
        var numbers = new SchoolPupilNumbers("100001",
            new PupilNumbersYears<int>(null, 40, null),
            new PupilNumbersYears<decimal>(90m, null, null));

        var result = NumberOfPupilsViewModel.ForYearByYear("id", 2024, [("School A", numbers)]);

        result.Schools.Single().EligiblePupils.Should().Equal("No available data", "40", "No available data");
        result.Schools.Single().ProportionIncluded.Should().Equal("90%", "No available data", "No available data");
    }

    [Fact]
    public void SchoolWithNoData_ShowsNoAvailableData()
    {
        var result = NumberOfPupilsViewModel.ForLatestYear("id", 2024, [("School A", null)]);

        result.Schools.Single().EligiblePupils.Should().Equal("No available data");
        result.Schools.Single().ProportionIncluded.Should().Equal("No available data");
    }

    [Fact]
    public void Schools_KeepTheOrderTheyAreGivenIn()
    {
        var result = NumberOfPupilsViewModel.ForLatestYear("id", 2024,
            [("First", Numbers(null)), ("Second", Numbers(null)), ("Third", Numbers(null))]);

        result.Schools.Select(s => s.Name).Should().Equal("First", "Second", "Third");
    }

    [Fact]
    public void MeasureNumberOfPupils_Create_BuildsEachViewWithAUniqueId()
    {
        var schools = new List<(string, SchoolPupilNumbers?)> { ("School A", Numbers(null)) };

        var result = MeasureNumberOfPupilsViewModel.Create("ks2-progress", 2024, schools, schools);

        result.CurrentYear.Id.Should().Be("ks2-progress-current-year-number-of-pupils");
        result.YearByYear.Id.Should().Be("ks2-progress-year-by-year-number-of-pupils");
        result.Table.Id.Should().Be("ks2-progress-table-number-of-pupils");
        result.TopPerformers!.Id.Should().Be("ks2-progress-top-performers-number-of-pupils");
        result.CurrentYear.YearLabels.Should().HaveCount(1);
        result.YearByYear.YearLabels.Should().HaveCount(3);
        result.Table.YearLabels.Should().HaveCount(3);
        result.TopPerformers.YearLabels.Should().HaveCount(1);
    }

    [Fact]
    public void MeasureNumberOfPupils_Create_WithoutTopPerformers_HasNoTopPerformersTable()
    {
        var schools = new List<(string, SchoolPupilNumbers?)> { ("School A", Numbers(null)) };

        var result = MeasureNumberOfPupilsViewModel.Create("ks2-progress", 2024, schools, null);

        result.TopPerformers.Should().BeNull();
    }
}
