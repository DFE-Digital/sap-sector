namespace SAPSec.Web.Authorization;

/// <summary>
/// How DfE staff are recognised. Bound from the "DfeUsers" configuration section.
/// </summary>
public class DfeUserOptions
{
    public const string SectionName = "DfeUsers";

    /// <summary>
    /// Names of the DfE Sign-in organisations that DfE staff sign in as (e.g. "Department for Education").
    /// </summary>
    public string[] OrganisationNames { get; set; } = [];
}
