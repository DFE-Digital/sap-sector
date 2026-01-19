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
    public static DataWithAvailability<string> MapString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DataWithAvailability<string>.NotAvailable();
        }

        return value.ToLower() switch
        {
            "c" => DataWithAvailability<string>.Redacted(),
            "z" => DataWithAvailability<string>.NotApplicable(),
            "x" => DataWithAvailability<string>.NotAvailable(),
            _ => DataWithAvailability<string>.Available(value)
        };
    }

    /// <summary>
    /// Maps a string value that cannot have special codes.
    /// </summary>
    public static DataWithAvailability<string> MapRequiredString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? DataWithAvailability<string>.NotAvailable()
            : DataWithAvailability<string>.Available(value);
    }

    /// <summary>
    /// Maps DfE number, treating "/" as empty.
    /// </summary>
    public static DataWithAvailability<string> MapDfENumber(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value == "/"
            ? DataWithAvailability<string>.NotAvailable()
            : DataWithAvailability<string>.Available(value);
    }

    /// <summary>
    /// Maps an age value from string to int.
    /// </summary>
    public static DataWithAvailability<int> MapAge(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DataWithAvailability<int>.NotAvailable();
        }

        return int.TryParse(value, out var age)
            ? DataWithAvailability<int>.Available(age)
            : DataWithAvailability<int>.NotAvailable();
    }

    /// <summary>
    /// Maps address from multiple fields.
    /// </summary>
    public static DataWithAvailability<string> MapAddress(Establishment establishment)
    {
        var parts = new List<string>();

        AddIfNotEmpty(parts, establishment.AddressStreet);
        AddIfNotEmpty(parts, establishment.AddressLocality);
        AddIfNotEmpty(parts, establishment.AddressAddress3);
        AddIfNotEmpty(parts, establishment.AddressTown);
        AddIfNotEmpty(parts, establishment.AddressPostcode);

        return parts.Count == 0
            ? DataWithAvailability<string>.NotAvailable()
            : DataWithAvailability<string>.Available(string.Join(", ", parts));
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

        return parts.Count == 0
            ? DataWithAvailability<string>.NotAvailable()
            : DataWithAvailability<string>.Available(string.Join(" ", parts));
    }

    /// <summary>
    /// Maps website URL, ensuring protocol is present.
    /// </summary>
    public static DataWithAvailability<string> MapWebsite(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DataWithAvailability<string>.NotAvailable();
        }

        var website = value.Trim();

        if (!website.StartsWith("http://") && !website.StartsWith("https://"))
        {
            website = $"https://{website}";
        }

        return DataWithAvailability<string>.Available(website);
    }

    /// <summary>
    /// Maps trust name, returning NotApplicable if no trust.
    /// </summary>
    public static DataWithAvailability<string> MapTrustName(Establishment establishment)
    {
        if (string.IsNullOrWhiteSpace(establishment.TrustsId))
        {
            return DataWithAvailability<string>.NotApplicable();
        }

        return string.IsNullOrWhiteSpace(establishment.TrustName)
            ? DataWithAvailability<string>.NotAvailable()
            : DataWithAvailability<string>.Available(establishment.TrustName);
    }

    /// <summary>
    /// Maps trust ID, returning NotApplicable if no trust.
    /// </summary>
    public static DataWithAvailability<string> MapTrustId(string? trustId)
    {
        return string.IsNullOrWhiteSpace(trustId)
            ? DataWithAvailability<string>.NotApplicable()
            : DataWithAvailability<string>.Available(trustId);
    }

    private static void AddIfNotEmpty(List<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value);
        }
    }
}