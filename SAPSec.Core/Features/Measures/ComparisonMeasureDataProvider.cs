using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public class ComparisonMeasureDataProvider<T>(
    IEstablishmentRepository establishmentRepository,
    IMeasureDataRepository<T> repository) : IComparisonMeasureDataProvider<T>
    where T : class, IMeasureData
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
