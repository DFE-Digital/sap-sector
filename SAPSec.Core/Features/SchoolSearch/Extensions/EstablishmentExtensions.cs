using SAPSec.Core.Constants;
using SAPSec.Core.Model.Generated;

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
        var phase = establishment?.PhaseOfEducationName?.Trim();

        return phase switch
        {
            _ when PhaseOfEducationValues.IsSecondary(phase) => true,
            _ when primarySchoolsEnabled && PhaseOfEducationValues.IsPrimaryOrAllThrough(phase) => true,
            _ => false
        };
    }
}
