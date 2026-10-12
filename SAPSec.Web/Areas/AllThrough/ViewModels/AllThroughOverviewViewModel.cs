using SAPSec.Web.Areas.Shared.ViewModels;

namespace SAPSec.Web.Areas.AllThrough.ViewModels;

public record AllThroughOverviewViewModel(SchoolViewModel School, bool HasPrimarySimilarSchools, bool HasSecondarySimilarSchools)
{
    public bool HasSimilarSchools => HasPrimarySimilarSchools || HasSecondarySimilarSchools;
}
