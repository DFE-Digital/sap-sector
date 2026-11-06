using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Caching;

public class CachedEstablishmentRepository(ICache cache, [FromKeyedServices(ServiceKeys.RealImplementation)] IEstablishmentRepository repository)
    : CachedRepository<IEstablishmentRepository>(cache, repository), IEstablishmentRepository
{
    public Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
        => CallCachedMethod(r => r.GetAllEstablishmentsAsync());

    public Task<Establishment?> GetEstablishmentAsync(string urn)
        => CallCachedMethod(r => r.GetEstablishmentAsync(urn));

    public Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
        => CallCachedMethod(r => r.GetEstablishmentByAnyNumberAsync(number));

    public Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
        => CallCachedMethod(r => r.GetEstablishmentEmailAsync(urn));

    public Task<IReadOnlyCollection<EstablishmentLinks>> GetEstablishmentLinksAsync(string urn)
        => CallCachedMethod(r => r.GetEstablishmentLinksAsync(urn));

    public Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
        => CallCachedMethod(r => r.GetEstablishmentsAsync(urns));
}