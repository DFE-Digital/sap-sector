using SAPSec.Data.Dto.KS4.Destinations;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryKs4DestinationsRepository(IEstablishmentRepository establishmentRepository) : IKs4DestinationsRepository
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

    public void ClearDown()
    {
        _establishment = [];
        _la = [];
        _england = [];
    }

    public async Task<Ks4DestinationsData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
        var ep = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == establishment?.LAId);
        var england = _england.FirstOrDefault(x => x.Id == "National");

        return establishment is null && la is null && england is null
            ? null
            : new Ks4DestinationsData(
                urn,
                ep,
                la,
                england);
    }

    public async Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);

        return establishments.Select(e => new Ks4DestinationsData(
                e.URN,
                _establishment.FirstOrDefault(x => x.Id == e.URN),
                _la.FirstOrDefault(x => x.Id == e.LAId),
                _england.FirstOrDefault(x => x.Id == "National")))
            .ToList();
    }
}
