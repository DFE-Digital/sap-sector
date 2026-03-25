using SAPSec.Data.Model.Generated;

namespace SAPSec.Data;

public interface IEstablishmentRepository
{
    Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync();
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns);
    Task<Establishment?> GetEstablishmentAsync(string urn);
    Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number);
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
