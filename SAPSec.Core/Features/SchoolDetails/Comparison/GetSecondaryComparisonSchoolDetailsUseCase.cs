using SAPSec.Core.Features.Geography;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails.Comparison;

public class GetSecondaryComparisonSchoolDetailsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    ISchoolDetailsService schoolDetailsService)
    : IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse>
{
    public async Task<GetComparisonSchoolDetailsResponse> Execute(GetComparisonSchoolDetailsRequest request)
    {
        var dataProvider = new ComparisonSchoolDetailsDataProvider<SimilarSchoolsSecondaryGroupsEntry, SimilarSchoolsSecondaryValuesEntry>(establishmentRepository, similarSchoolsRepository);

        var (currentSchool, comparatorSchool) = await dataProvider.GetData(request.CurrentSchoolUrn, request.ComparatorSchoolUrn);

        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.ComparatorSchoolUrn);

        return new(
            new SchoolWithCoordinates(currentSchool.SchoolInfo, currentSchool.Data.ToGeographicCoordinates()),
            new SchoolWithCoordinates(comparatorSchool.SchoolInfo, comparatorSchool.Data.ToGeographicCoordinates()),
            currentSchool.Data?.DistanceMiles(comparatorSchool.Data),
            similarSchoolDetails);
    }
}