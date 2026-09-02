using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class GetComparisonAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse>
{
    public async Task<GetComparisonAttendanceMeasuresResponse> Execute(GetComparisonAttendanceMeasuresRequest request)
    {
        var dataProvider = new ComparisonMeasureDataProvider<AbsenceData>(
            establishmentRepository,
            absenceRepository);

        var (currentSchoolData, comparatorSchoolData) = await dataProvider.GetData(
            request.CurrentSchoolUrn,
            request.ComparatorSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolData.SchoolInfo,
            comparatorSchoolData.SchoolInfo,
            AttendanceMeasures.Absence.ForSchoolComparison(
                request.Phase,
                currentSchoolData,
                comparatorSchoolData,
                filterBy));
    }
}

public record GetComparisonAttendanceMeasuresRequest(
    MeasurePhase Phase,
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonAttendanceMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo ComparatorSchool,
    Measure Absence);
