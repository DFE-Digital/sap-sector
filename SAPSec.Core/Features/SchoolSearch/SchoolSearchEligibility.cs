using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch;

public static class SchoolSearchEligibility
{
    private static readonly string[] SearchablePhases = ["Primary", "Secondary"];

    public static bool IsSearchable(Establishment? establishment)
    {
        var phase = establishment?.PhaseOfEducationName?.Trim();

        if (string.IsNullOrWhiteSpace(phase))
            return false;

        return SearchablePhases.Any(allowedPhase =>
            phase.Contains(allowedPhase, StringComparison.OrdinalIgnoreCase));
    }
}
