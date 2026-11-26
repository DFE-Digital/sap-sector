using SAPSec.Data.Dto;

namespace SAPSec.Data.Repositories;

public record EstablishmentSearchHit(string URN, string EstablishmentName);

public interface IEstablishmentRepository
{
    Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync();
    Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns);
    Task<Establishment?> GetEstablishmentAsync(string urn);
    Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number);
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
    Task<IReadOnlyCollection<EstablishmentLinks>> GetEstablishmentLinksAsync(string urn);

    Task<IReadOnlyCollection<EstablishmentSearchHit>> SearchEstablishmentsAsync(string query, int maxResults, bool isBroadTerm);
}
