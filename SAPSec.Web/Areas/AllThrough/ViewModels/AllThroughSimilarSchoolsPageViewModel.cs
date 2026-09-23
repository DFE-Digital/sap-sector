using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.AllThrough.ViewModels;

public record AllThroughSimilarSchoolsPageViewModel(
    SchoolInfoViewModel School,
    string SelectedPhase,
    string PrimaryTabUrl,
    string SecondaryTabUrl,
    SimilarSchoolsPageViewModel? PrimarySimilarSchools,
    bool HasPrimarySimilarSchools,
    bool HasSecondarySimilarSchools)
{
    public bool IsPrimarySelected => SelectedPhase.Equals("primary", StringComparison.OrdinalIgnoreCase);

    public string WhatIsASimilarSchoolUrl => Routes.AllThroughSchool(School.Urn).WhatIsASimilarSchool;

    public bool ShouldShowPrimaryEmptyState => IsPrimarySelected && !HasPrimarySimilarSchools && HasSecondarySimilarSchools;

    public static AllThroughSimilarSchoolsPageViewModel FromSchoolInfo(
        Core.Features.SchoolInfo.SchoolInfo school,
        string? phase,
        string primaryTabUrl,
        string secondaryTabUrl,
        SimilarSchoolsPageViewModel? primarySimilarSchools,
        bool hasPrimarySimilarSchools,
        bool hasSecondarySimilarSchools)
    {
        var selectedPhase = phase?.Equals("secondary", StringComparison.OrdinalIgnoreCase) == true
            ? "secondary"
            : "primary";

        return new(
            SchoolInfoViewModel.FromSchoolInfo(school),
            selectedPhase,
            primaryTabUrl,
            secondaryTabUrl,
            primarySimilarSchools,
            hasPrimarySimilarSchools,
            hasSecondarySimilarSchools);
    }
}
