namespace SAPSec.Data.Repositories;

public interface IMeasureDataRepository<T>
    where T : IMeasureData
{
    Task<T?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<T>> GetByUrnsAsync(IEnumerable<string> urns);
}
