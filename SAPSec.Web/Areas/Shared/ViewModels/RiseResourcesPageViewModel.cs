using SAPSec.Core.Features.RiseResources;

namespace SAPSec.Web.Areas.Shared.ViewModels;

public sealed class RiseResourceItemViewModel
{
}

public sealed class RiseResourcesPageViewModel
{
    public required SchoolInfoViewModel School { get; init; }
    public IReadOnlyList<RiseResourceItemViewModel> Resources { get; init; } = [];

    public static RiseResourcesPageViewModel FromResponse(GetRiseResourcesResponse response) =>
        new()
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School)
        };
}
