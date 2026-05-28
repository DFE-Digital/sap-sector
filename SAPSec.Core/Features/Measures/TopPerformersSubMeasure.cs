namespace SAPSec.Core.Features.Measures;

public record TopPerformersSubMeasure(
    IEnumerable<TopPerformer> TopPerformers)
    : SubMeasure
{
    internal static TopPerformersSubMeasure ForSchool(
        SchoolData currentSchool,
        IEnumerable<SchoolData> similarSchools,
        MeasureFieldSelector fieldSelector)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.Urn,
            currentSchool.Name,
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool),
                fieldSelector.SchoolPrevious(currentSchool),
                fieldSelector.SchoolPrevious2(currentSchool)),
            IsCurrentSchool: true);

        return new(similarSchools
            .Select(x => new TopPerformerCandidate(
                x.Urn,
                x.Name,
                MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(x),
                    fieldSelector.SchoolPrevious(x),
                    fieldSelector.SchoolPrevious2(x)),
                IsCurrentSchool: false))
            .Append(currentSchoolCandidate)
            .Where(x => x.Value.HasValue)
            .GroupBy(x => x.Urn, StringComparer.Ordinal)
            .Select(x => x.OrderByDescending(candidate => candidate.IsCurrentSchool).First())
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new TopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly());
    }

    private sealed record TopPerformerCandidate(
        string Urn,
        string Name,
        decimal? Value,
        bool IsCurrentSchool);
}

public record TopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false);
