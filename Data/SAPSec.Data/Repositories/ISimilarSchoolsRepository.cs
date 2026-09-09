namespace SAPSec.Data.Repositories;

public interface ISimilarSchoolsRepository<TGroupsEntry, TValuesEntry>
    where TGroupsEntry : ISimilarSchoolsGroupsEntry
    where TValuesEntry : ISimilarSchoolsValuesEntry
{
    Task<IReadOnlyCollection<TGroupsEntry>> GetGroupAsync(string urn);
    Task<IReadOnlyCollection<TValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet();
}
