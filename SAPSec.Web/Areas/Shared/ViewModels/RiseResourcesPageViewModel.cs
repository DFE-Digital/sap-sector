using SAPSec.Core.Features.RiseResources;

namespace SAPSec.Web.Areas.Shared.ViewModels;

public sealed class RiseResourceItemViewModel
{
}

public sealed class RiseResourcesPageViewModel
{
    public required string SchoolUrn { get; init; }
    public required string SchoolName { get; init; }
    public IReadOnlyList<RiseResourceItemViewModel> Resources { get; init; } = [];

    public static RiseResourcesPageViewModel FromResponse(GetRiseResourcesResponse response) =>
        new()
        {
            SchoolUrn = response.Urn,
            SchoolName = response.SchoolName            
        };
}
