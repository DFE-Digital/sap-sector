using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4PerformanceRepository
{
    Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4HeadlineMeasuresByUrn>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4HeadlineMeasuresByUrn(string Urn, Ks4HeadlineMeasuresData? Data);

public record Ks4HeadlineMeasuresData(
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

