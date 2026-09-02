using SAPSec.Core.Features.Geography;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails.Comparison;

internal class ComparisonSchoolDetailsDataProvider<TGroupsEntry, TValuesEntry>(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsRepository<TGroupsEntry, TValuesEntry> similarSchoolsRepository)
    where TGroupsEntry : ISimilarSchoolsGroupsEntry
    where TValuesEntry : ISimilarSchoolsValuesEntry
{
    public async Task<ComparisonSchoolData<BNGCoordinates>> GetData(string currentSchoolUrn, string comparatorSchoolUrn)
    {
        var establishments = (await establishmentRepository.GetEstablishmentsAsync(
                [currentSchoolUrn, comparatorSchoolUrn]))
            .ToDictionary(e => e.URN, StringComparer.Ordinal);

        if (!establishments.TryGetValue(currentSchoolUrn, out var currentEstablishment))
        {
            throw new NotFoundException($"School not found with URN {currentSchoolUrn}");
        }

        if (!establishments.TryGetValue(comparatorSchoolUrn, out var comparatorEstablishment))
        {
            throw new NotFoundException($"School not found with URN {comparatorSchoolUrn}");
        }

        var group = await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn);

        if (!group.Any(g => g.NeighbourURN == comparatorSchoolUrn))
        {
            throw new NotFoundException($"School with URN {comparatorSchoolUrn} is not in similar schools group for school with URN {currentSchoolUrn}");
        }

        var currentSchool = new SchoolData<BNGCoordinates>(
            SchoolInfo.SchoolInfo.FromEstablishment(currentEstablishment),
            BNGCoordinates.TryParse(currentEstablishment.Easting, currentEstablishment.Northing, out var currentCoords)
                ? currentCoords
                : null);

        var comparatorSchool = new SchoolData<BNGCoordinates>(
            SchoolInfo.SchoolInfo.FromEstablishment(comparatorEstablishment),
            BNGCoordinates.TryParse(
            comparatorEstablishment.Easting, comparatorEstablishment.Northing, out var comparatorCoords)
            ? comparatorCoords
            : null);

        return new(currentSchool, comparatorSchool);
    }
}