using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public class ComparisonMeasureDataProvider<T, TGroupsEntry, TValuesEntry>(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsRepository<TGroupsEntry, TValuesEntry> similarSchoolsRepository,
    IMeasureDataRepository<T> repository) : IComparisonMeasureDataProvider<T>
    where T : class, IMeasureData
    where TGroupsEntry : ISimilarSchoolsGroupsEntry
    where TValuesEntry : ISimilarSchoolsValuesEntry
{
    public async Task<ComparisonMeasureData<T>> GetData(
        string currentSchoolUrn,
        string comparatorSchoolUrn)
    {
        string[] urns = [currentSchoolUrn, comparatorSchoolUrn];

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        if (!schools.ContainsKey(comparatorSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {comparatorSchoolUrn}");
        }

        var group = await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn);

        if (!group.Any(g => g.NeighbourURN == comparatorSchoolUrn))
        {
            throw new NotFoundException($"School with URN {comparatorSchoolUrn} is not in similar schools group for school with URN {currentSchoolUrn}");
        }

        var absence = (await repository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolMeasureData<T>(
            schools[currentSchoolUrn],
            absence.TryGetValue(currentSchoolUrn, out var currentAbsence) ? currentAbsence : null);

        var comparatorSchoolData = new SchoolMeasureData<T>(
            schools[comparatorSchoolUrn],
            absence.TryGetValue(comparatorSchoolUrn, out var similarAbsence) ? similarAbsence : null);

        return new(currentSchoolData, comparatorSchoolData);
    }
}
