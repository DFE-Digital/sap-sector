using SAPSec.Core.Model.KS4.Performance;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4PerformanceRepository
{
    Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn);
}

public record Ks4HeadlineMeasuresData(
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance,
    int? EstablishmentTotalPupils,
    int? EnglandTotalPupils);
