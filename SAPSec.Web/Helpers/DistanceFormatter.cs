public static class DistanceFormatter
{
    private const double MetresPerMile = 1609.344;

    public static string DistanceMilesOrNoData(int? e1, int? n1, int? e2, int? n2)
    {
        if (!HasCoords(e1, n1) || !HasCoords(e2, n2))
            return "No available data";

        var dx = (double)(e2!.Value - e1!.Value);
        var dy = (double)(n2!.Value - n1!.Value);

        var metres = Math.Sqrt((dx * dx) + (dy * dy));
        var miles = metres / MetresPerMile;

        // standard rounding, 1dp
        var rounded = Math.Round(miles, 1, MidpointRounding.AwayFromZero);

        return $"{rounded:0.0} miles";
    }

    private static bool HasCoords(int? e, int? n) =>
        e.HasValue && n.HasValue;
}