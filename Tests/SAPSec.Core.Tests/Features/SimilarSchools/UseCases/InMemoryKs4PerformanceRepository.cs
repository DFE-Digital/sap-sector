using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class InMemoryKs4PerformanceRepository : IKs4PerformanceRepository
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

    public Task<Ks4PerformanceData?> GetByUrnAsync(string urn)
        => Task.FromResult(GetByUrn(urn));

    private Ks4PerformanceData? GetByUrn(string urn)
    {
        var establishment = _establishment.FirstOrDefault(x => x.Id == urn);
        var la = _la.FirstOrDefault(x => x.Id == urn);
        var england = _england.FirstOrDefault(x => x.Id == urn);

        return establishment is null && la is null && england is null
            ? null
            : new Ks4PerformanceData(
                urn,
                establishment,
                la,
                england);
    }

    public Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<Ks4PerformanceData>)urns.Select(GetByUrn).Where(x => x is not null).ToList());
}
