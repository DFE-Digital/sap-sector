namespace SAPSec.Web.Authorization;

/// <summary>
/// How DfE staff are recognised. Bound from the "DfeUsers" configuration section.
/// </summary>
public class DfeUserOptions
{
    public const string SectionName = "DfeUsers";

    /// <summary>
    /// Email domains that identify DfE staff (e.g. "education.gov.uk"). Matched exactly, so a subdomain such as
    /// "digital.education.gov.uk" must be listed separately if it should count.
    /// </summary>
    public string[] EmailDomains { get; set; } = [];
}
