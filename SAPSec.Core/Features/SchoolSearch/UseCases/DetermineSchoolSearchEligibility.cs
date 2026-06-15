using SAPSec.Core.Constants;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch.UseCases;

public class DetermineSchoolSearchEligibility
{
    public bool CanIndex(Establishment? establishment)
    {
        if (establishment == null)
        {
            return false;
        }

        if (PhaseOfEducationValues.IsSearchableIndexPhaseId(establishment.PhaseOfEducationId))
        {
            return true;
        }

        return HasLegacySearchablePhaseName(establishment.PhaseOfEducationName);
    }

    public bool CanSearch(
        Establishment? establishment,
        EstablishmentEmail? establishmentEmail,
        bool primarySchoolsEnabled)
    {
        if (establishment == null)
        {
            return false;
        }

        return HasSearchablePhase(establishment, primarySchoolsEnabled)
            && EstablishmentStatusValues.IsIncludedInSearch(
                establishmentEmail?.EstablishmentStatusId,
                establishmentEmail?.EstablishmentStatusName);
    }

    private static bool HasSearchablePhase(Establishment establishment, bool primarySchoolsEnabled)
    {
        if (PhaseOfEducationValues.IsSearchableSearchPhaseId(establishment.PhaseOfEducationId, primarySchoolsEnabled))
        {
            return true;
        }

        var phase = establishment.PhaseOfEducationName;

        return PhaseOfEducationValues.IsSecondary(phase)
            || (primarySchoolsEnabled && PhaseOfEducationValues.IsPrimaryOrAllThrough(phase));
    }

    private static bool HasLegacySearchablePhaseName(string? phaseOfEducationName)
    {
        return PhaseOfEducationValues.IsSecondary(phaseOfEducationName)
            || PhaseOfEducationValues.IsPrimaryOrAllThrough(phaseOfEducationName);
    }
}
