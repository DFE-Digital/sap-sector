using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Test.Common.InMemoryStore;

public class InMemoryKs4DestinationsStore : IKs4DestinationsStore
{
    private List<EstablishmentDestinations> _establishment = new();
    private List<LADestinations> _la = new();
    private List<EnglandDestinations> _england = new();

    public void SetupEstablishmentDestinations(params EstablishmentDestinations[] establishment)
    {
        _establishment = establishment.ToList();
    }

    public void SetupLADestinations(params LADestinations[] la)
    {
        _la = la.ToList();
    }

    public void SetupEnglandDestinations(params EnglandDestinations[] england)
    {
        _england = england.ToList();
    }

    public Task<Ks4DestinationsData?> GetByUrnAsync(string urn)
        => Task.FromResult(GetByUrn(urn));

    private Ks4DestinationsData? GetByUrn(string urn)
    {
        var establishment = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == urn);
        var england = _england.FirstOrDefault(x => x.Id == urn);

        return establishment is null && la is null && england is null
            ? null
            : new Ks4DestinationsData(
                urn,
                establishment,
                la,
                england);
    }

    public Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<Ks4DestinationsData>)urns.Select(GetByUrn).Where(x => x is not null).ToList());
}
