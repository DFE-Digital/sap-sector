using SAPSec.Data.Dto;

namespace SAPSec.Data.Repositories;

public interface IEstablishmentRepository
{
    Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync();
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns);
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsByTrustIdAsync(string trustId);
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsByLocalAuthorityIdAsync(string laId);
    Task<Establishment?> GetEstablishmentAsync(string urn);
    Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number);
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
    Task<IReadOnlyCollection<EstablishmentLinks>> GetEstablishmentLinksAsync(string urn);
}
