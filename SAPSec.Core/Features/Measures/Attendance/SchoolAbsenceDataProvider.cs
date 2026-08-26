using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class SchoolAbsenceDataProvider(
    IAbsenceRepository absenceRepository,
    IEstablishmentRepository establishmentRepository)
{
    public async Task<SchoolData<AbsenceData>> GetData(string currentSchoolUrn)
    {
        var school = await establishmentRepository.GetEstablishmentAsync(currentSchoolUrn);

        if (school is null)
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var absence = await absenceRepository.GetByUrnAsync(school.URN);

        return new SchoolData<AbsenceData>(
            SchoolInfo.SchoolInfo.FromEstablishment(school),
            absence);
    }
}
