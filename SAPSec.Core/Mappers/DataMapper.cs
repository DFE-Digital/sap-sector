using SAPSec.Core.Model;

namespace SAPSec.Core.Mappers;

/// <summary>
/// Maps raw establishment data to DataWithAvailability values.
/// Single Responsibility: Only handles basic data mapping, no business logic.
/// </summary>
public static class DataMapper
{
    /// <summary>
    /// Maps a string value to DataWithAvailability, handling GIAS special codes.
    /// </summary>
    public static DataWithAvailability<string> MapString(string? value) =>
        DataWithAvailability.FromStringWithCodes(value);

    /// <summary>
    /// Maps a string value that cannot have special codes.
    /// </summary>
    public static DataWithAvailability<string> MapRequiredString(string? value) =>
        DataWithAvailability.FromStringWithoutCodes(value);

    /// <summary>
    /// Maps DfE number, treating "/" as empty.
    /// </summary>
    public static DataWithAvailability<string> MapDfENumber(string? value) =>
        value == "/"
            ? DataWithAvailability.NotAvailable<string>()
            : DataWithAvailability.FromStringWithoutCodes(value);

    /// <summary>
    /// Maps an age value from string to int.
    /// </summary>
    public static DataWithAvailability<int> MapAge(string? value)
        => DataWithAvailability.FromIntegerString(value);

    /// <summary>
    /// Maps address from multiple fields.
    /// </summary>
    public static DataWithAvailability<string> MapAddress(Establishment establishment)
    {
        var parts = new List<string>();

        AddIfNotEmpty(parts, establishment.Street);
        AddIfNotEmpty(parts, establishment.Locality);
        AddIfNotEmpty(parts, establishment.Address3);
        AddIfNotEmpty(parts, establishment.Town);
        AddIfNotEmpty(parts, establishment.Postcode);

        return DataWithAvailability.FromStringWithoutCodes(string.Join(", ", parts));
    }

    /// <summary>
    /// Maps headteacher name from multiple fields.
    /// </summary>
    public static DataWithAvailability<string> MapHeadteacher(Establishment establishment)
    {
        var parts = new List<string>();

        AddIfNotEmpty(parts, establishment.HeadteacherTitle);
        AddIfNotEmpty(parts, establishment.HeadteacherFirstName);
        AddIfNotEmpty(parts, establishment.HeadteacherLastName);

        return DataWithAvailability.FromStringWithoutCodes(string.Join(" ", parts));
    }

    /// <summary>
    /// Maps website URL, ensuring protocol is present.
    /// </summary>
    public static DataWithAvailability<string> MapWebsite(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DataWithAvailability.NotAvailable<string>();
        }

        var website = value.Trim();

        if (!website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !website.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            website = $"https://{website}";
        }

        return DataWithAvailability.Available(website);
    }

    /// <summary>
    /// Maps trust name, returning NotApplicable if no trust.
    /// </summary>
    public static DataWithAvailability<string> MapTrustName(Establishment establishment)
    {
        if (string.IsNullOrWhiteSpace(establishment.TrustsId))
        {
            return DataWithAvailability.NotApplicable<string>();
        }

        return DataWithAvailability.FromStringWithoutCodes(establishment.TrustName);
    }

    /// <summary>
    /// Maps trust ID, returning NotApplicable if no trust.
    /// </summary>
    public static DataWithAvailability<string> MapTrustId(string? trustId)
    {
        return string.IsNullOrWhiteSpace(trustId)
            ? DataWithAvailability.NotApplicable<string>()
            : DataWithAvailability.Available(trustId);
    }


    private static void AddIfNotEmpty(List<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value);
        }
    }
}