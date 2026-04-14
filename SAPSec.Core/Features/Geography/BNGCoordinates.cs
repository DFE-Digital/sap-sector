using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core.Features.Geography;

public record BNGCoordinates(int Easting, int Northing)
{
    private const double KilometersToMiles = 0.6213712;

    public static bool TryParse([NotNullWhen(true)] int? easting, [NotNullWhen(true)] int? northing, [NotNullWhen(true)] out BNGCoordinates? coordinates)
    {
        if (easting.HasValue && northing.HasValue)
        {
            coordinates = new BNGCoordinates(easting.Value, northing.Value);
            return true;
        }

        coordinates = null;
        return false;
    }

    public double DistanceMiles(BNGCoordinates other)
    {
        var e = (Easting - other.Easting) / 1000.0 * KilometersToMiles;
        var n = (Northing - other.Northing) / 1000.0 * KilometersToMiles;

        return Math.Sqrt(e * e + n * n);
    }
}
