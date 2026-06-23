using SAPSec.Core.Constants;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SchoolSearch.Extensions;

public static class EstablishmentExtensions
{
    public static bool IsSearchable(this Establishment? establishment, bool primarySchoolsEnabled)
    {
        if (establishment == null)
        {
            return false;
        }

        if (!PhaseOfEducationValues.IsSearchable(
            establishment.PhaseOfEducationId,
            establishment.PhaseOfEducationName,
            primarySchoolsEnabled))
        {
            return false;
        }

        return EstablishmentStatusValues.IsSearchable(
            establishment.EstablishmentStatusId,
            establishment.EstablishmentStatusName);
    }
}
