using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Repositories
{
    public interface ILookupRepository
    {
        Task<IEnumerable<Lookup>> GetAllLookupsAsync();
        Task<Lookup?> GetLookupAsync(string urn);
    }
}
