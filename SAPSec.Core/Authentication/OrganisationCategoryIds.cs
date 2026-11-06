namespace SAPSec.Core.Authentication;

/// <summary>
/// DfE Sign-in organisation category ids.
/// </summary>
public static class OrganisationCategoryIds
{
    public const string Establishment = "001";
    public const string LocalAuthority = "002";
    public const string MultiAcademyTrust = "010";
    public const string SingleAcademyTrust = "013";

    // PLACEHOLDER: the DSI category id for a Diocese is not confirmed yet, so Dioceses are
    // recognised by category name. Replace with the real id (and match on Category.Id like the
    // others) once it is known from a real DSI claim.
    public const string DioceseCategoryName = "Diocese";
}
