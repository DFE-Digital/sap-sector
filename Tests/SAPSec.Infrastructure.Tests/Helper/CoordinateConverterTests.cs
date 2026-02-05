using FluentAssertions;
using SAPSec.Core.Features.SchoolSearch;

namespace SAPSec.Infrastructure.Tests.Helper;

public class CoordinateConverterTests
{
    [Fact]
    public void EastingNorthingToLatLon_WithValidLondonCoordinates_ReturnsCorrectLatLon()
    {
        // Arrange - London (Big Ben approximate coordinates)
        double easting = 530047;
        double northing = 179951;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(51.5007, 0.01, "Latitude should be approximately 51.5 degrees North");
        longitude.Should().BeApproximately(-0.1246, 0.01, "Longitude should be approximately 0.12 degrees West");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithValidLeedsCoordinates_ReturnsCorrectLatLon()
    {
        // Arrange - Leeds City Centre
        double easting = 430000;
        double northing = 433000;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(53.8, 0.1, "Latitude should be approximately 53.8 degrees North");
        longitude.Should().BeApproximately(-1.55, 0.1, "Longitude should be approximately 1.55 degrees West");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithValidManchesterCoordinates_ReturnsCorrectLatLon()
    {
        // Arrange - Manchester City Centre
        double easting = 384000;
        double northing = 398000;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(53.48, 0.1, "Latitude should be approximately 53.48 degrees North");
        longitude.Should().BeApproximately(-2.24, 0.1, "Longitude should be approximately 2.24 degrees West");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithValidEdinburghCoordinates_ReturnsCorrectLatLon()
    {
        // Arrange - Edinburgh Castle
        double easting = 325776;
        double northing = 673919;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(55.95, 0.01, "Latitude should be approximately 55.95 degrees North");
        longitude.Should().BeApproximately(-3.19, 0.01, "Longitude should be approximately 3.19 degrees West");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithValidBrightonCoordinates_ReturnsCorrectLatLon()
    {
        // Arrange - Brighton Pier
        double easting = 531000;
        double northing = 104000;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(50.82, 0.01, "Latitude should be approximately 50.82 degrees North");
        longitude.Should().BeApproximately(-0.14, 0.01, "Longitude should be approximately 0.14 degrees West");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithZeroCoordinates_ReturnsSpecificLatLon()
    {
        // Arrange - Origin of British National Grid
        double easting = 0;
        double northing = 0;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().NotBe(0, "Latitude should not be zero for BNG origin");
        longitude.Should().NotBe(0, "Longitude should not be zero for BNG origin");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithMaxBritishGridCoordinates_ReturnsValidLatLon()
    {
        // Arrange - Near max valid BNG coordinates
        double easting = 700000;
        double northing = 1250000;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeInRange(49, 62, "Latitude should be within British Isles range");
        longitude.Should().BeInRange(-8, 4, "Longitude should be within British Isles range including eastern extents");
    }

    [Fact]
    public void EastingNorthingToLatLon_ReturnsLatitudeFirst_ThenLongitude()
    {
        // Arrange
        double easting = 530047;
        double northing = 179951;

        // Act
        var result = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        result.Latitude.Should().BeGreaterThan(result.Longitude,
            "For UK coordinates, latitude (51) should typically be greater than longitude (negative or small positive)");
    }

    [Fact]
    public void EastingNorthingToLatLon_WithSchoolTypicalCoordinates_ReturnsValidLatLon()
    {
        // Arrange - Typical school location in Yorkshire
        double easting = 445000;
        double northing = 450000;

        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeInRange(50, 56, "Latitude should be in typical UK school range");
        longitude.Should().BeInRange(-4, 2, "Longitude should be in typical UK school range");
        latitude.Should().BeGreaterThan(49, "Latitude should be north of southernmost UK point");
        longitude.Should().BeLessThan(2, "Longitude should be west of or near Greenwich");
    }

    [Theory]
    [InlineData(326000, 674000, 55.95, -3.2)]    // Edinburgh
    [InlineData(530000, 180000, 51.5, -0.12)]    // London
    [InlineData(430000, 433000, 53.8, -1.55)]    // Leeds
    public void EastingNorthingToLatLon_WithKnownLocations_ReturnsExpectedValues(
        double easting, double northing, double expectedLat, double expectedLon)
    {
        // Act
        var (latitude, longitude) = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        latitude.Should().BeApproximately(expectedLat, 0.1);
        longitude.Should().BeApproximately(expectedLon, 0.1);
    }

    [Fact]
    public void EastingNorthingToLatLon_ConsistentResults_ForSameInput()
    {
        // Arrange
        double easting = 430000;
        double northing = 433000;

        // Act
        var result1 = CoordinateConverter.EastingNorthingToLatLon(easting, northing);
        var result2 = CoordinateConverter.EastingNorthingToLatLon(easting, northing);

        // Assert
        result1.Latitude.Should().Be(result2.Latitude, "Multiple calls with same input should return same latitude");
        result1.Longitude.Should().Be(result2.Longitude, "Multiple calls with same input should return same longitude");
    }

    [Fact]
    public void EastingNorthingToLatLon_DifferentInputs_ProduceDifferentOutputs()
    {
        // Arrange
        double easting1 = 430000, northing1 = 433000;
        double easting2 = 530000, northing2 = 180000;

        // Act
        var result1 = CoordinateConverter.EastingNorthingToLatLon(easting1, northing1);
        var result2 = CoordinateConverter.EastingNorthingToLatLon(easting2, northing2);

        // Assert
        result1.Latitude.Should().NotBe(result2.Latitude, "Different inputs should produce different latitudes");
        result1.Longitude.Should().NotBe(result2.Longitude, "Different inputs should produce different longitudes");
    }
}
