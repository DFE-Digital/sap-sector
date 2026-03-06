using FluentAssertions;
using SAPSec.Core.Mappers;
using SAPSec.Core.Model;
using Xunit;

namespace SAPSec.Core.Tests.Mappers;

public class DataMapperTests
{
    #region MapString Tests

    [Fact]
    public void MapString_ValidValue_ReturnsAvailable()
    {
        // Act
        var result = DataMapper.MapString("Test Value");

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Test Value");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MapString_EmptyValue_ReturnsNotAvailable(string? value)
    {
        // Act
        var result = DataMapper.MapString(value);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    [Fact]
    public void MapString_RedactedCode_ReturnsRedacted()
    {
        // Act
        var result = DataMapper.MapString("c");

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.Redacted);
    }

    [Fact]
    public void MapString_RedactedCodeUpperCase_ReturnsRedacted()
    {
        // Act
        var result = DataMapper.MapString("C");

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.Redacted);
    }

    [Fact]
    public void MapString_NotApplicableCode_ReturnsNotApplicable()
    {
        // Act
        var result = DataMapper.MapString("z");

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    [Fact]
    public void MapString_NotAvailableCode_ReturnsNotAvailable()
    {
        // Act
        var result = DataMapper.MapString("x");

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapRequiredString Tests

    [Fact]
    public void MapRequiredString_ValidValue_ReturnsAvailable()
    {
        // Act
        var result = DataMapper.MapRequiredString("Test");

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Test");
    }

    [Fact]
    public void MapRequiredString_SpecialCode_TreatsAsValue()
    {
        // Act - "c" should NOT be treated as redacted for required strings
        var result = DataMapper.MapRequiredString("c");

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("c");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MapRequiredString_Empty_ReturnsNotAvailable(string? value)
    {
        // Act
        var result = DataMapper.MapRequiredString(value);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapDfENumber Tests

    [Fact]
    public void MapDfENumber_ValidValue_ReturnsAvailable()
    {
        // Act
        var result = DataMapper.MapDfENumber("373/1234");

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("373/1234");
    }

    [Fact]
    public void MapDfENumber_SlashOnly_ReturnsNotAvailable()
    {
        // Act
        var result = DataMapper.MapDfENumber("/");

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MapDfENumber_Empty_ReturnsNotAvailable(string? value)
    {
        // Act
        var result = DataMapper.MapDfENumber(value);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapAge Tests

    [Theory]
    [InlineData("5", 5)]
    [InlineData("11", 11)]
    [InlineData("16", 16)]
    [InlineData("0", 0)]
    public void MapAge_ValidNumber_ReturnsAvailable(string input, int expected)
    {
        // Act
        var result = DataMapper.MapAge(input);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12.5")]
    public void MapAge_InvalidValue_ReturnsNotAvailable(string? value)
    {
        // Act
        var result = DataMapper.MapAge(value);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapAddress Tests

    [Fact]
    public void MapAddress_AllFields_ReturnsFormattedAddress()
    {
        // Arrange
        var establishment = new Establishment
        {
            Street = "123 Test Street",
            Locality = "Test Area",
            Town = "Sheffield",
            Postcode = "S1 1AA"
        };

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("123 Test Street, Test Area, Sheffield, S1 1AA");
    }

    [Fact]
    public void MapAddress_PartialFields_ReturnsAvailableFields()
    {
        // Arrange
        var establishment = new Establishment
        {
            Town = "Sheffield",
            Postcode = "S1 1AA"
        };

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Sheffield, S1 1AA");
    }

    [Fact]
    public void MapAddress_OnlyPostcode_ReturnsPostcode()
    {
        // Arrange
        var establishment = new Establishment
        {
            Postcode = "S1 1AA"
        };

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("S1 1AA");
    }

    [Fact]
    public void MapAddress_NoFields_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment();

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    [Fact]
    public void MapAddress_EmptyStrings_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment
        {
            Street = "",
            Town = "  ",
            Postcode = null
        };

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    [Fact]
    public void MapAddress_IncludesAddress3_WhenPresent()
    {
        // Arrange
        var establishment = new Establishment
        {
            Street = "123 Street",
            Locality = "Locality",
            Address3 = "District",
            Town = "Sheffield",
            Postcode = "S1 1AA"
        };

        // Act
        var result = DataMapper.MapAddress(establishment);

        // Assert
        result.Value.Should().Be("123 Street, Locality, District, Sheffield, S1 1AA");
    }

    #endregion

    #region MapHeadteacher Tests

    [Fact]
    public void MapHeadteacher_AllFields_ReturnsFullName()
    {
        // Arrange
        var establishment = new Establishment
        {
            HeadTitle = "Mrs",
            HeadFirstName = "Jane",
            HeadLastName = "Smith"
        };

        // Act
        var result = DataMapper.MapHeadteacher(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Mrs Jane Smith");
    }

    [Fact]
    public void MapHeadteacher_NoTitle_ReturnsNameOnly()
    {
        // Arrange
        var establishment = new Establishment
        {
            HeadFirstName = "Jane",
            HeadLastName = "Smith"
        };

        // Act
        var result = DataMapper.MapHeadteacher(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Jane Smith");
    }

    [Fact]
    public void MapHeadteacher_LastNameOnly_ReturnsLastName()
    {
        // Arrange
        var establishment = new Establishment
        {
            HeadLastName = "Smith"
        };

        // Act
        var result = DataMapper.MapHeadteacher(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Smith");
    }

    [Fact]
    public void MapHeadteacher_NoFields_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment();

        // Act
        var result = DataMapper.MapHeadteacher(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapWebsite Tests

    [Theory]
    [InlineData("https://school.org.uk", "https://school.org.uk")]
    [InlineData("http://school.org.uk", "http://school.org.uk")]
    [InlineData("school.org.uk", "https://school.org.uk")]
    [InlineData("www.school.org.uk", "https://www.school.org.uk")]
    public void MapWebsite_AddsProtocolIfMissing(string input, string expected)
    {
        // Act
        var result = DataMapper.MapWebsite(input);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void MapWebsite_TrimsWhitespace()
    {
        // Act
        var result = DataMapper.MapWebsite("  www.school.org.uk  ");

        // Assert
        result.Value.Should().Be("https://www.school.org.uk");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MapWebsite_Empty_ReturnsNotAvailable(string? value)
    {
        // Act
        var result = DataMapper.MapWebsite(value);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapTrustName Tests

    [Fact]
    public void MapTrustName_WithTrust_ReturnsAvailable()
    {
        // Arrange
        var establishment = new Establishment
        {
            TrustsId = "5001",
            TrustName = "Test Academy Trust"
        };

        // Act
        var result = DataMapper.MapTrustName(establishment);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("Test Academy Trust");
    }

    [Fact]
    public void MapTrustName_NoTrustId_ReturnsNotApplicable()
    {
        // Arrange
        var establishment = new Establishment
        {
            TrustsId = null,
            TrustName = "Test Academy Trust"
        };

        // Act
        var result = DataMapper.MapTrustName(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    [Fact]
    public void MapTrustName_EmptyTrustId_ReturnsNotApplicable()
    {
        // Arrange
        var establishment = new Establishment
        {
            TrustsId = "",
            TrustName = "Test Academy Trust"
        };

        // Act
        var result = DataMapper.MapTrustName(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    [Fact]
    public void MapTrustName_TrustIdButNoName_ReturnsNotAvailable()
    {
        // Arrange
        var establishment = new Establishment
        {
            TrustsId = "5001",
            TrustName = null
        };

        // Act
        var result = DataMapper.MapTrustName(establishment);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotAvailable);
    }

    #endregion

    #region MapTrustId Tests

    [Fact]
    public void MapTrustId_WithId_ReturnsAvailable()
    {
        // Act
        var result = DataMapper.MapTrustId("5001");

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Value.Should().Be("5001");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MapTrustId_NoId_ReturnsNotApplicable(string? trustId)
    {
        // Act
        var result = DataMapper.MapTrustId(trustId);

        // Assert
        result.Availability.Should().Be(DataAvailabilityStatus.NotApplicable);
    }

    #endregion
}