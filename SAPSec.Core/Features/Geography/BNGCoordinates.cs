using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SAPSec.Core.Features.Geography;

public record BNGCoordinates(double Easting, double Northing)
{
    private const double KilometersToMiles = 0.6213712;

    public static bool TryParse([NotNullWhen(true)] string? easting, [NotNullWhen(true)] string? northing, [NotNullWhen(true)] out BNGCoordinates? coordinates)
    {
        if (double.TryParse(easting, NumberStyles.Any, CultureInfo.InvariantCulture, out var e) &&
            double.TryParse(northing, NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
        {
            coordinates = new BNGCoordinates(e, n);
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
