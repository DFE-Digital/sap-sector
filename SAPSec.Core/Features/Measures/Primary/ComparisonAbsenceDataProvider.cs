using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Primary;

public class ComparisonAbsenceDataProvider(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
{
    public async Task<(SchoolData<AbsenceData> CurrentSchool, SchoolData<AbsenceData> SimilarSchool)> GetComparisonAbsence(
        string currentSchoolUrn,
        string similarSchoolUrn)
    {
        var urns = new[] { currentSchoolUrn, similarSchoolUrn };

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        if (!schools.ContainsKey(similarSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {similarSchoolUrn}");
        }

        var absences = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<AbsenceData>(
            schools[currentSchoolUrn],
            absences.TryGetValue(currentSchoolUrn, out var currentAbsence) ? currentAbsence : null);

        var similarSchoolData = new SchoolData<AbsenceData>(
            schools[similarSchoolUrn],
            absences.TryGetValue(similarSchoolUrn, out var similarAbsence) ? similarAbsence : null);

        return (currentSchoolData, similarSchoolData);
    }
}
