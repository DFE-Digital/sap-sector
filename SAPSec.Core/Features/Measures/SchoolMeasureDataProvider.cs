using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public class SchoolMeasureDataProvider<T>(
    IEstablishmentRepository establishmentRepository,
    IMeasureDataRepository<T> repository) : IMeasureDataProvider<T>
    where T : class, IMeasureData
{
    public async Task<SchoolMeasureData<T>> GetData(string currentSchoolUrn)
    {
        var school = await establishmentRepository.GetEstablishmentAsync(currentSchoolUrn);

        if (school is null)
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var data = await repository.GetByUrnAsync(school.URN);

        return new SchoolMeasureData<T>(
            SchoolInfo.SchoolInfo.FromEstablishment(school),
            data);
    }
}
