using SAPSec.Data.Dto;

namespace SAPSec.Core.SchoolSearch.Extensions;

public static class EstablishmentExtensions
{
    private static readonly string[] SearchablePhases = ["Primary", "Secondary"];

    public static bool IsSearchable(this Establishment? establishment)
    {
        var phase = establishment?.PhaseOfEducationName?.Trim();

        if (string.IsNullOrWhiteSpace(phase))
            return false;

        return SearchablePhases.Any(allowedPhase =>
            phase.Contains(allowedPhase, StringComparison.OrdinalIgnoreCase));
    }
}
