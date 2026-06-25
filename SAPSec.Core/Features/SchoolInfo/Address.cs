using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public record Address(
    string? Street = null,
    string? Locality = null,
    string? Address3 = null,
    string? Town = null,
    string? Postcode = null)
{
    public override string ToString()
    {
        var parts = new List<string>();

        AddIfNotEmpty(parts, Street);
        AddIfNotEmpty(parts, Locality);
        AddIfNotEmpty(parts, Address3);
        AddIfNotEmpty(parts, Town);
        AddIfNotEmpty(parts, Postcode);

        return string.Join(", ", parts);
    }

    public static Address FromEstablishment(Establishment establishment) => new(
        establishment.Street,
        establishment.Locality,
        establishment.Address3,
        establishment.Town,
        establishment.Postcode);

    private static void AddIfNotEmpty(List<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value);
        }
    }
}
