using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryKs2PerformanceRepository(IEstablishmentRepository establishmentRepository) : IKs2PerformanceRepository
{
    private List<EstablishmentPerformance> _establishment = new();
    private List<LAPerformance> _la = new();
    private List<EnglandPerformance> _england = new();

    public InMemoryKs2PerformanceRepository SetupEstablishmentPerformance(params EstablishmentPerformance[] establishment)
    {
        _establishment = establishment.ToList();

        return this;
    }

    public InMemoryKs2PerformanceRepository SetupLAPerformance(params LAPerformance[] la)
    {
        _la = la.ToList();

        return this;
    }

    public InMemoryKs2PerformanceRepository SetupEnglandPerformance(params EnglandPerformance[] england)
    {
        _england = england.ToList();

        return this;
    }

    public InMemoryKs2PerformanceRepository ClearDown()
    {
        _establishment = [];
        _la = [];
        _england = [];

        return this;
    }

    public async Task<Ks2PerformanceData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
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
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);

        return establishments.Select(e => new Ks2PerformanceData(
                e.URN,
                _establishment.FirstOrDefault(x => x.Id == e.URN),
                _la.FirstOrDefault(x => x.Id == e.LAId),
                _england.FirstOrDefault(x => x.Id == "National")))
            .ToList();
    }
}
