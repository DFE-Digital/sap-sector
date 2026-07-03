namespace SAPSec.Core.Constants;

/// <summary>
/// Trust school flag field values and helper methods.
/// Used to determine goverance structure of a school.
/// </summary>
public static class EstablishmentGroupTypeValues
{
    public const string LaMaintainedSchool = "4";

    public static bool IsLaMaintainedSchool(string establishmentTypeGroupId)
    {
        var trimmedEstablishmentTypeGroupId = establishmentTypeGroupId?.Trim();

        if (trimmedEstablishmentTypeGroupId is LaMaintainedSchool)
            return true;

        return false;
    }
}