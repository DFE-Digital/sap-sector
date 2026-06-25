using SAPSec.Core.Features.Primary;
using SAPSec.Core.Measures;

namespace SAPSec.Core.Features.Measures;

public record TopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false)
{
    internal static IEnumerable<TopPerformer> ForSecondarySchool<T>(
    SchoolData<T> currentSchool,
    IEnumerable<SchoolData<T>> similarSchools,
    MeasureFieldSelector<T> fieldSelector)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.SchoolInfo.Urn,
            currentSchool.SchoolInfo.Name,
            MeasureHelper.AverageFrom(
                fieldSelector.SchoolCurrent(currentSchool.Data),
                fieldSelector.SchoolPrevious(currentSchool.Data),
                fieldSelector.SchoolPrevious2(currentSchool.Data)),
            IsCurrentSchool: true);

        return similarSchools
            .Select(x => new TopPerformerCandidate(
                x.SchoolInfo.Urn,
                x.SchoolInfo.Name,
                MeasureHelper.AverageFrom(
                    fieldSelector.SchoolCurrent(x.Data),
                    fieldSelector.SchoolPrevious(x.Data),
                    fieldSelector.SchoolPrevious2(x.Data)),
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
            .AsReadOnly();
    }

    private sealed record TopPerformerCandidate(
        string Urn,
        string Name,
        decimal? Value,
        bool IsCurrentSchool);
}
