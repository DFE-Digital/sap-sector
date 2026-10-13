namespace SAPSec.Web.ViewModels;

public class YourSchoolsViewModel : SchoolSearchResultsViewModel
{
    public required string OrganisationName { get; init; }

    /// <summary>
    /// How the organisation is described in page text, e.g. "trust" or "local authority".
    /// </summary>
    public required string OrganisationTypeDescription { get; init; }
}
