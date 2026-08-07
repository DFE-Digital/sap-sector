using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class PrimaryAttendanceComparisonDataProvider(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
{
    public async Task<(SchoolData<AbsenceData> CurrentSchool, SchoolData<AbsenceData> SimilarSchool)> GetComparisonAttendance(
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

        var attendances = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<AbsenceData>(
            schools[currentSchoolUrn],
            attendances.TryGetValue(currentSchoolUrn, out var currentAttendance) ? currentAttendance : null);

        var similarSchoolData = new SchoolData<AbsenceData>(
            schools[similarSchoolUrn],
            attendances.TryGetValue(similarSchoolUrn, out var similarAttendance) ? similarAttendance : null);

        return (currentSchoolData, similarSchoolData);
    }
}
