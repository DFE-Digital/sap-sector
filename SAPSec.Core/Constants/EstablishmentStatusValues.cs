namespace SAPSec.Core.Constants;

public static class EstablishmentStatusValues
{
    public const string OpenId = "1";
    public const string ClosedId = "2";
    public const string OpenButProposedToCloseId = "3";
    public const string ProposedToOpenId = "4";

    public const string Open = "Open";
    public const string Closed = "Closed";
    public const string OpenButProposedToClose = "Open, but proposed to close";
    public const string ProposedToOpen = "Proposed to open";

    public static bool IsSearchable(string? statusId, string? statusName)
    {
        if (string.IsNullOrWhiteSpace(statusId) && string.IsNullOrWhiteSpace(statusName))
        {
            return false;
        }

        var trimmedStatusId = statusId?.Trim();

        if (trimmedStatusId is OpenId or OpenButProposedToCloseId)
        {
            return true;
        }

        if (trimmedStatusId is ClosedId or ProposedToOpenId)
        {
            return false;
        }

        var trimmedStatusName = statusName?.Trim();

        return string.Equals(trimmedStatusName, Open, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmedStatusName, OpenButProposedToClose, StringComparison.OrdinalIgnoreCase);
    }
}
