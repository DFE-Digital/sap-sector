using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Data.Repositories;

public record Ks4PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance) : IMeasureData;

