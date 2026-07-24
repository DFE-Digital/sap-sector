using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryAbsenceRepository(IEstablishmentRepository establishmentRepository) : IAbsenceRepository
{
    private List<EstablishmentAbsence> _establishment = new();
    private List<LAAbsence> _la = new();
    private List<EnglandAbsence> _england = new();

    public void SetupEstablishmentAbsence(params EstablishmentAbsence[] establishment)
    {
        _establishment = establishment.ToList();
    }

    public void SetupLAAbsence(params LAAbsence[] la)
    {
        _la = la.ToList();
    }

    public void SetupEnglandAbsence(params EnglandAbsence[] england)
    {
        _england = england.ToList();
    }

    public void ClearDown()
    {
        _establishment = [];
        _la = [];
        _england = [];
    }

    //public Task<AbsenceData?> GetByUrnAsync(string urn)
    //    => Task.FromResult(GetByUrn(urn));

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

    public Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<AbsenceData>)urns.Select(GetByUrnAsync).Where(x => x is not null).ToList());
}