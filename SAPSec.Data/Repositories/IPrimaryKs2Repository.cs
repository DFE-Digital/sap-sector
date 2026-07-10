using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Data.Repositories;

public interface IPrimaryKs2Repository
{
    Task<PrimaryKs2Data?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<PrimaryKs2Data>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record PrimaryKs2Data(
    string URN,
    EstablishmentPerformance? EstablishmentPerformance,
    IReadOnlyCollection<EstablishmentSubjectEntries> EstablishmentSubjectEntries,
    LAPerformance? LocalAuthorityPerformance,
    IReadOnlyCollection<LASubjectEntries> LocalAuthoritySubjectEntries,
    EnglandPerformance? EnglandPerformance);
