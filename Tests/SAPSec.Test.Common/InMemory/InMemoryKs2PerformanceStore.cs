using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Store;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryKs2PerformanceStore(IEstablishmentStore establishmentStore) : IKs2PerformanceStore
{
    private List<EstablishmentPerformance> _establishment = new();
    private List<LAPerformance> _la = new();
    private List<EnglandPerformance> _england = new();

    public void SetupEstablishmentPerformance(params EstablishmentPerformance[] establishment)
    {
        _establishment = establishment.ToList();
    }

    public void SetupLAPerformance(params LAPerformance[] la)
    {
        _la = la.ToList();
    }

    public void SetupEnglandPerformance(params EnglandPerformance[] england)
    {
        _england = england.ToList();
    }

    public void ClearDown()
    {
        _establishment = [];
        _la = [];
        _england = [];
    }

    public async Task<Ks2PerformanceData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentStore.GetEstablishmentAsync(urn);
        var ep = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == establishment?.LAId);
        var england = _england.FirstOrDefault(x => x.Id == "National");

        return establishment is null && la is null && england is null
            ? null
            : new Ks2PerformanceData(
                urn,
                ep,
                la,
                england);
    }

    public async Task<IReadOnlyCollection<Ks2PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var establishments = await establishmentStore.GetEstablishmentsAsync(urns);

        return establishments.Select(e => new Ks2PerformanceData(
                e.URN,
                _establishment.FirstOrDefault(x => x.Id == e.URN),
                _la.FirstOrDefault(x => x.Id == e.LAId),
                _england.FirstOrDefault(x => x.Id == "National")))
            .ToList();
    }
}
