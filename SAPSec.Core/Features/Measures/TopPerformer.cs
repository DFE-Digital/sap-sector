using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public record TopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false)
{
    internal static IReadOnlyCollection<TopPerformer> BuildTopPerformers<T>(
        SchoolMeasureData<T> currentSchool,
        IEnumerable<SchoolMeasureData<T>> similarSchools,
        MeasureFieldSelector<T> fieldSelector,
        MeasureDataType dataType)
        where T : class, IMeasureData
    {
        return similarSchools
            // Include current school in list
            .Append(currentSchool)
            .Select(x => new TopPerformerCandidate(
                x.SchoolInfo.Urn,
                x.SchoolInfo.Name,
                MeasureHelper.ParseNullableDecimal(fieldSelector.SchoolCurrent(x.Data)),
                IsCurrentSchool: x == currentSchool))

            // Exclude missing data
            .Where(x => x.Value.HasValue)

            // Remove duplicates if school with the same URN already appears in the list,
            // prioritising current school
            // TODO: not sure this will ever happen?
            .GroupBy(x => x.Urn, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())

            .OrderByDescending(x => TopPerformerSortValue(x.Value, dataType))
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new TopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();
    }

    private static decimal TopPerformerSortValue(decimal? value, MeasureDataType dataType) =>
        Math.Round(value!.Value, DisplayDecimalPlaces(dataType), MidpointRounding.AwayFromZero);

    private static int DisplayDecimalPlaces(MeasureDataType dataType) =>
        dataType is MeasureDataType.Score or MeasureDataType.ScaledScore ? 1 : 0;

    private sealed record TopPerformerCandidate(
        string Urn,
        string Name,
        decimal? Value,
        bool IsCurrentSchool);
}
