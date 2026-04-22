using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Test.Common.Repositories.InMemory;

public class InMemoryEstablishmentRepository : IEstablishmentRepository
{
    private List<Establishment> _establishments = new();
    private List<EstablishmentEmail> _establishmentEmails = new();

    public void SetupEstablishments(params Establishment[] establishments)
    {
        _establishments = establishments.ToList();
    }

    public void SetupEstablishmentEmails(params EstablishmentEmail[] establishmentEmails)
    {
        _establishmentEmails = establishmentEmails.ToList();
    }

    public Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
        => Task.FromResult((IReadOnlyCollection<Establishment>)_establishments);

    public Task<Establishment?> GetEstablishmentAsync(string urn)
        => Task.FromResult(_establishments.FirstOrDefault(i => i.URN == urn));

    public Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
        => Task.FromResult(_establishments.FirstOrDefault(x => x.URN == number || x.UKPRN == number || x.LAESTAB == number));

    public Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<Establishment>)_establishments.Where(x => urns.Contains(x.URN)).ToList());

    public Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
        => Task.FromResult(_establishmentEmails.FirstOrDefault(i => i.URN == urn));
}
