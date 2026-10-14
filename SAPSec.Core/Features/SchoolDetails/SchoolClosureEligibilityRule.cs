using SAPSec.Core.Time;
using SAPSec.Data.Dto;
using System.Globalization;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Pure data-eligibility rule: whether a closed school still meets the agreed 3-year data
/// availability window. This is independent of the successor/predecessor banner rule, which
/// additionally needs the establishment's links and so cannot be a pure function of the
/// establishment alone - see <see cref="SchoolClosureEligibilityService"/>.
/// </summary>
public class SchoolClosureEligibilityRule(ICurrentTimeProvider timeProvider)
{
    public const string CloseDateFormat = "dd-MM-yyyy";

    // Cutoff date for eligibility is 12th September every year
    public const int CutoffDay = 12;
    public const int CutoffMonth = 9;

    /// <summary>
    /// True if the establishment may still appear in the service: it is not closed, or it is
    /// closed but within the data eligibility window (or its close date is unknown).
    /// </summary>
    public bool IsEligibleToAppear(Establishment establishment)
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
    /// date is more than 3 years older than the cutoff date. An unknown close date is treated as eligible - we cannot
    /// prove a school is ineligible without knowing when it closed.
    /// </summary>
    private bool IsBeyondDataEligibilityWindow(string? closeDate)
    {
        if (!DateOnly.TryParseExact(closeDate, CloseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedCloseDate))
        {
            return false;
        }

        var cutoff = GetElegibilityCutoffDate();

        return parsedCloseDate < cutoff;
    }

    private DateOnly GetElegibilityCutoffDate()
    {
        var now = timeProvider.GetCurrentTimeUtc();

        // Always use most recent cutoff date minus 3 years
        var mostRecentCutoffDate = now.Month < CutoffMonth || now.Month == CutoffMonth && now.Day < CutoffDay
            ? new DateOnly(now.Year - 1, CutoffMonth, CutoffDay)
            : new DateOnly(now.Year, CutoffMonth, CutoffDay);

        return mostRecentCutoffDate.AddYears(-3);
    }
}
