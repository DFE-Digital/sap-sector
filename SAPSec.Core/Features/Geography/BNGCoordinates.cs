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
        var (e, n) = (Easting - other.Easting, Northing - other.Northing);

        return Math.Sqrt(e * e + n * n) / 1000.0 * KilometersToMiles;
    }
}
