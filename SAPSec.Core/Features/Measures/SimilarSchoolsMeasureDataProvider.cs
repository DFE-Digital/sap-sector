using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public class SimilarSchoolsMeasureDataProvider<T, TGroupsEntry, TValuesEntry>(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsRepository<TGroupsEntry, TValuesEntry> similarSchoolsRepository,
    IMeasureDataRepository<T> repository) : ISimilarSchoolsMeasureDataProvider<T>
    where T : class, IMeasureData
    where TGroupsEntry : ISimilarSchoolsGroupsEntry
    where TValuesEntry : ISimilarSchoolsValuesEntry
{
    public async Task<SimilarSchoolsMeasureData<T>> GetData(string currentSchoolUrn)
    {
        var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var currentSchool = schools[currentSchoolUrn];

        var performances = (await repository.GetByUrnsAsync(schools.Keys))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolMeasureData<T>(
            currentSchool,
            performances[currentSchoolUrn]);

        var similarSchoolsData = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolMeasureData<T>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsMeasureData<T>(
            currentSchoolData,
            similarSchoolsData);
    }
}

