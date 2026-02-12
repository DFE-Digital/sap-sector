using FluentAssertions;
using SAPSec.Core.Model;
using SAPSec.Web.Helpers;
using Xunit;

namespace SAPSec.Web.Tests.Helpers;

public class DisplayHelpersTests
{
    #region Display (string) Tests

    [Fact]
    public void Display_AvailableString_ReturnsValue()
    {
        // Arrange
        var data = DataWithAvailability.Available("Test Value");

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("Test Value");
    }

    [Fact]
    public void Display_NotAvailableString_ReturnsDefaultText()
    {
        // Arrange
        var data = DataWithAvailability.NotAvailable<string>();

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("No available data");
    }

    [Fact]
    public void Display_RedactedString_ReturnsRedactedText()
    {
        // Arrange
        var data = DataWithAvailability.Redacted<string>();

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("Data has been redacted");
    }

    [Fact]
    public void Display_NotApplicableString_ReturnsNotApplicableText()
    {
        // Arrange
        var data = DataWithAvailability.NotApplicable<string>();

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("Not applicable");
    }

    [Fact]
    public void Display_LowQualityString_ReturnsValue()
    {
        // Arrange
        var data = DataWithAvailability.Low("Low Quality Value");

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("Low Quality Value");
    }

    [Fact]
    public void Display_AvailableStringWithNullValue_ReturnsDefaultText()
    {
        // Arrange - This shouldn't happen in practice, but testing defensively
        var data = DataWithAvailability.Available<string>(null!);

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("No available data");
    }

    #endregion

    #region DisplayAs (bool) Tests

    [Fact]
    public void DisplayAs_TrueValue_ReturnsTrueText()
    {
        // Arrange
        var data = DataWithAvailability.Available(true);

        // Act
        var result = data.DisplayAs("Has it", "Does not have it");

        // Assert
        result.Should().Be("Has it");
    }

    [Fact]
    public void DisplayAs_FalseValue_ReturnsFalseText()
    {
        // Arrange
        var data = DataWithAvailability.Available(false);

        // Act
        var result = data.DisplayAs("Has it", "Does not have it");

        // Assert
        result.Should().Be("Does not have it");
    }

    [Fact]
    public void DisplayAs_NotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var data = DataWithAvailability.NotAvailable<bool>();

        // Act
        var result = data.DisplayAs("Has it", "Does not have it");

        // Assert
        result.Should().Be("No available data");
    }

    [Fact]
    public void DisplayAs_NotApplicable_ReturnsNotApplicableText()
    {
        // Arrange
        var data = DataWithAvailability.NotApplicable<bool>();

        // Act
        var result = data.DisplayAs("Has it", "Does not have it");

        // Assert
        result.Should().Be("Not applicable");
    }

    [Fact]
    public void DisplayAs_Redacted_ReturnsRedactedText()
    {
        // Arrange
        var data = DataWithAvailability.Redacted<bool>();

        // Act
        var result = data.DisplayAs("Has it", "Does not have it");

        // Assert
        result.Should().Be("Data has been redacted");
    }

    #endregion

    #region Display (GovernanceType) Tests

    [Theory]
    [InlineData(GovernanceType.MultiAcademyTrust, "Multi-academy trust (MAT)")]
    [InlineData(GovernanceType.SingleAcademyTrust, "Single-academy trust (SAT)")]
    [InlineData(GovernanceType.LocalAuthorityMaintained, "Local authority maintained")]
    [InlineData(GovernanceType.NonMaintainedSpecialSchool, "Non-maintained special school")]
    [InlineData(GovernanceType.Independent, "Independent")]
    [InlineData(GovernanceType.FurtherHigherEducation, "Further/Higher education")]
    [InlineData(GovernanceType.Other, "Other")]
    public void Display_GovernanceType_ReturnsCorrectText(GovernanceType type, string expected)
    {
        // Arrange
        var data = DataWithAvailability.Available(type);

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Display_GovernanceTypeNotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var data = DataWithAvailability.NotAvailable<GovernanceType>();

        // Act
        var result = data.Display();

        // Assert
        result.Should().Be("No available data");
    }

    #endregion

    #region DisplayAgeRange Tests

    [Fact]
    public void DisplayAgeRange_BothAvailable_ReturnsRange()
    {
        // Arrange
        var low = DataWithAvailability.Available(11);
        var high = DataWithAvailability.Available(18);

        // Act
        var result = low.DisplayAgeRange(high);

        // Assert
        result.Should().Be("11 to 18");
    }

    [Fact]
    public void DisplayAgeRange_OnlyLowAvailable_ReturnsLowOnly()
    {
        // Arrange
        var low = DataWithAvailability.Available(11);
        var high = DataWithAvailability.NotAvailable<int>();

        // Act
        var result = low.DisplayAgeRange(high);

        // Assert
        result.Should().Be("11");
    }

    [Fact]
    public void DisplayAgeRange_LowNotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var low = DataWithAvailability.NotAvailable<int>();
        var high = DataWithAvailability.Available(18);

        // Act
        var result = low.DisplayAgeRange(high);

        // Assert
        result.Should().Be("No available data");
    }

    [Fact]
    public void DisplayAgeRange_BothNotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var low = DataWithAvailability.NotAvailable<int>();
        var high = DataWithAvailability.NotAvailable<int>();

        // Act
        var result = low.DisplayAgeRange(high);

        // Assert
        result.Should().Be("No available data");
    }

    [Fact]
    public void DisplayAgeRange_ZeroValues_ReturnsRange()
    {
        // Arrange
        var low = DataWithAvailability.Available(0);
        var high = DataWithAvailability.Available(5);

        // Act
        var result = low.DisplayAgeRange(high);

        // Assert
        result.Should().Be("0 to 5");
    }

    #endregion

    #region DisplayWithCode Tests

    [Fact]
    public void DisplayWithCode_BothAvailable_ReturnsNameWithCode()
    {
        // Arrange
        var name = DataWithAvailability.Available("Sheffield");
        var code = DataWithAvailability.Available("373");

        // Act
        var result = name.DisplayWithCode(code);

        // Assert
        result.Should().Be("Sheffield (373)");
    }

    [Fact]
    public void DisplayWithCode_OnlyNameAvailable_ReturnsNameOnly()
    {
        // Arrange
        var name = DataWithAvailability.Available("Sheffield");
        var code = DataWithAvailability.NotAvailable<string>();

        // Act
        var result = name.DisplayWithCode(code);

        // Assert
        result.Should().Be("Sheffield");
    }

    [Fact]
    public void DisplayWithCode_NameNotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var name = DataWithAvailability.NotAvailable<string>();
        var code = DataWithAvailability.Available("373");

        // Act
        var result = name.DisplayWithCode(code);

        // Assert
        result.Should().Be("No available data");
    }

    [Fact]
    public void DisplayWithCode_BothNotAvailable_ReturnsDefaultText()
    {
        // Arrange
        var name = DataWithAvailability.NotAvailable<string>();
        var code = DataWithAvailability.NotAvailable<string>();

        // Act
        var result = name.DisplayWithCode(code);

        // Assert
        result.Should().Be("No available data");
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void Display_NurseryProvision_HasNursery()
    {
        // Arrange
        var data = DataWithAvailability.Available(true);

        // Act
        var result = data.DisplayAs("Has nursery classes", "Does not have nursery classes");

        // Assert
        result.Should().Be("Has nursery classes");
    }

    [Fact]
    public void Display_NurseryProvision_NoNursery()
    {
        // Arrange
        var data = DataWithAvailability.Available(false);

        // Act
        var result = data.DisplayAs("Has nursery classes", "Does not have nursery classes");

        // Assert
        result.Should().Be("Does not have nursery classes");
    }

    [Fact]
    public void Display_SixthForm_HasSixthForm()
    {
        // Arrange
        var data = DataWithAvailability.Available(true);

        // Act
        var result = data.DisplayAs("Has a sixth form", "Does not have a sixth form");

        // Assert
        result.Should().Be("Has a sixth form");
    }

    [Fact]
    public void Display_SenUnit_HasSenUnit()
    {
        // Arrange
        var data = DataWithAvailability.Available(true);

        // Act
        var result = data.DisplayAs("Has a SEN unit", "Does not have a SEN unit");

        // Assert
        result.Should().Be("Has a SEN unit");
    }

    #endregion
}