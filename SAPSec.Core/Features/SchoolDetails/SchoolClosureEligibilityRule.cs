using System.Globalization;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Pure data-eligibility rule: whether a closed school still meets the agreed 4-year data
/// availability window. This is independent of the successor/predecessor banner rule, which
/// additionally needs the establishment's links and so cannot be a pure function of the
/// establishment alone - see <see cref="SchoolClosureEligibilityService"/>.
/// </summary>
public static class SchoolClosureEligibilityRule
{
    private const int DataEligibilityWindowYears = 4;
    private const string CloseDateFormat = "dd-MM-yyyy";

    /// <summary>
    /// True if the establishment may still appear in the service: it is not closed, or it is
    /// closed but within the data eligibility window (or its close date is unknown).
    /// </summary>
    public static bool IsEligibleToAppear(Establishment establishment)
    {
        ArgumentNullException.ThrowIfNull(establishment);

        if (!EstablishmentStatusValues.IsClosed(establishment.EstablishmentStatusId, establishment.EstablishmentStatusName))
        {
            return true;
        }

        return !IsBeyondDataEligibilityWindow(establishment.CloseDate);
    }

    /// <summary>
    /// A school is beyond the data eligibility window only when we know its close date and that
    /// date is more than 4 years ago. An unknown close date is treated as eligible - we cannot
    /// prove a school is ineligible without knowing when it closed.
    /// </summary>
    private static bool IsBeyondDataEligibilityWindow(string? closeDate)
    {
        if (!DateOnly.TryParseExact(closeDate, CloseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedCloseDate))
        {
            return false;
        }

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-DataEligibilityWindowYears);

        return parsedCloseDate < cutoff;
    }
}
