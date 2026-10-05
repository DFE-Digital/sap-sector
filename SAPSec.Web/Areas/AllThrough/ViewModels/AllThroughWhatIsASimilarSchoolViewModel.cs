using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.AllThrough.ViewModels;

public record AllThroughWhatIsASimilarSchoolViewModel(
    SchoolInfoViewModel School,
    bool HasPrimarySimilarSchools,
    bool HasSecondarySimilarSchools)
{
    public bool HasSimilarSchools => HasPrimarySimilarSchools || HasSecondarySimilarSchools;
    public string OverviewUrl => Routes.AllThroughSchool(School.Urn).Overview;
    public string ViewSimilarSchoolsUrl => Routes.AllThroughSchool(School.Urn).ViewSimilarSchools;
}
