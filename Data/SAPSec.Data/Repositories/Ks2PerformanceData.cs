using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Data.Repositories;

public record Ks2PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance) : IMeasureData;