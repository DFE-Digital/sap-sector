namespace SAPSec.Web.ViewModels.Measures;

/// <summary>
/// Works out a chart axis for progress scores, which can be positive or negative with no fixed range
/// (most are between -5 and 5, but outliers such as -25 or 30 are possible).
/// The axis always includes 0 so bars diverge from a central baseline, and uses a "nice" step so
/// there are no more than <see cref="MaxIntervals"/> gridlines.
/// </summary>
public static class ProgressScoreAxis
{
    private const int MaxIntervals = 8;

    private static readonly decimal[] Steps = [0.5m, 1m, 2m, 5m, 10m, 20m, 50m, 100m];

    public static (decimal Min, decimal Step, decimal Max) For(IEnumerable<decimal?> values)
    {
        var available = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();

        var low = Math.Min(0m, available.DefaultIfEmpty(0m).Min());
        var high = Math.Max(0m, available.DefaultIfEmpty(0m).Max());

        if (low == 0m && high == 0m)
        {
            return (-1m, 0.5m, 1m);
        }

        foreach (var step in Steps)
        {
            var min = Math.Floor(low / step) * step;
            var max = Math.Ceiling(high / step) * step;

            if ((max - min) / step <= MaxIntervals)
            {
                return (min, step, max);
            }
        }

        var largest = Steps[^1];
        return (Math.Floor(low / largest) * largest, largest, Math.Ceiling(high / largest) * largest);
    }
}
