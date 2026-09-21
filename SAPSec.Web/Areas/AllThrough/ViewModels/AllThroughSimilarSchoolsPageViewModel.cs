using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.AllThrough.ViewModels;

public record AllThroughSimilarSchoolsPageViewModel(SchoolInfoViewModel School, string SelectedPhase)
{
    public bool IsPrimarySelected => SelectedPhase.Equals("primary", StringComparison.OrdinalIgnoreCase);

    public string PrimaryTabUrl => Routes.AllThroughSchool(School.Urn).ViewSimilarSchools;

    public string SecondaryTabUrl => $"{Routes.AllThroughSchool(School.Urn).ViewSimilarSchools}?phase=secondary";

    public string WhatIsASimilarSchoolUrl => Routes.AllThroughSchool(School.Urn).WhatIsASimilarSchool;

    public static AllThroughSimilarSchoolsPageViewModel FromSchoolInfo(
        Core.Features.SchoolInfo.SchoolInfo school,
        string? phase)
    {
        var selectedPhase = phase?.Equals("secondary", StringComparison.OrdinalIgnoreCase) == true
            ? "secondary"
            : "primary";

        return new(SchoolInfoViewModel.FromSchoolInfo(school), selectedPhase);
    }
}
