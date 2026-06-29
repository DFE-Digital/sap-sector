using SAPSec.Data.Dto;

namespace SAPSec.Data.Store;

public interface IEstablishmentStore
{
    Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync();
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns);
    Task<Establishment?> GetEstablishmentAsync(string urn);
    Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number);
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
