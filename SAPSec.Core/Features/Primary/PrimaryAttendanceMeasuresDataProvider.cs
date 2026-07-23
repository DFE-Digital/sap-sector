using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class PrimaryAttendanceMeasuresDataProvider(
    IAbsenceRepository attendanceRepository,
    IEstablishmentRepository establishmentRepository)
{
    public async Task<SchoolData<AbsenceData>> GetSchoolAttendance(string currentSchoolUrn)
    {
        var school = await establishmentRepository.GetEstablishmentAsync(currentSchoolUrn);

        if (school is null)
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var attendances = (await attendanceRepository.GetByUrnAsync(school.URN));

        var temp = new SchoolData<AbsenceData>(
         SchoolInfo.SchoolInfo.FromEstablishment(school),
         attendances);

        return temp;
    }
}
