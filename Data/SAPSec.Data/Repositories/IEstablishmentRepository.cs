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

    /// <summary>
    /// Full text search over establishment name, street and postcode. All tokens must match; the last
    /// token is a prefix match (typeahead). Only establishments that can be indexed for search are returned.
    /// </summary>
    Task<IReadOnlyCollection<EstablishmentSearchHit>> SearchEstablishmentsAsync(string query, int maxResults);
}
