using SAPSec.Core.Constants;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolSearch.Extensions;

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

    public static bool IsSearchable(this Establishment? establishment, bool primarySchoolsEnabled)
    {
        var phase = establishment?.PhaseOfEducationName;

        return PhaseOfEducationValues.IsSecondary(phase)
            || (primarySchoolsEnabled && PhaseOfEducationValues.IsPrimaryOrAllThrough(phase));
    }
}
