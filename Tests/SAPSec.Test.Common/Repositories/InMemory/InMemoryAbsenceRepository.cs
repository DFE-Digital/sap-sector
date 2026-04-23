using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Test.Common.Repositories.InMemory;

public class InMemoryAbsenceRepository : IAbsenceRepository
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

    public Task<AbsenceData?> GetByUrnAsync(string urn)
        => Task.FromResult(GetByUrn(urn));

    private AbsenceData? GetByUrn(string urn)
    {
        var establishment = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == urn);
        var england = _england.FirstOrDefault(x => x.Id == urn);

        return establishment is null && la is null && england is null
            ? null
            : new AbsenceData(
                urn,
                establishment,
                la,
                england);
    }

    public Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<AbsenceData>)urns.Select(GetByUrn).Where(x => x is not null).ToList());
}