using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryAbsenceRepository(IEstablishmentRepository establishmentRepository) : IAbsenceRepository
{
    private List<EstablishmentAbsence> _establishment = new();
    private List<LAAbsence> _la = new();
    private List<EnglandAbsence> _england = new();

    public InMemoryAbsenceRepository SetupEstablishmentAbsence(params EstablishmentAbsence[] establishment)
    {
        _establishment = establishment.ToList();

        return this;
    }

    public InMemoryAbsenceRepository SetupLAAbsence(params LAAbsence[] la)
    {
        _la = la.ToList();

        return this;
    }

    public InMemoryAbsenceRepository SetupEnglandAbsence(params EnglandAbsence[] england)
    {
        _england = england.ToList();

        return this;
    }

    public InMemoryAbsenceRepository ClearDown()
    {
        _establishment = [];
        _la = [];
        _england = [];

        return this;
    }

    public async Task<AbsenceData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
        var ep = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == establishment?.LAId);
        var england = _england.FirstOrDefault(x => x.Id == "National");

        return establishment is null && la is null && england is null
            ? null
            : new AbsenceData(
                urn,
                ep,
                la,
                england);
    }

    public async Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);

        return establishments.Select(e => new AbsenceData(
                e.URN,
                _establishment.FirstOrDefault(x => x.Id == e.URN),
                _la.FirstOrDefault(x => x.Id == e.LAId),
                _england.FirstOrDefault(x => x.Id == "National")))
            .ToList();
    }
}