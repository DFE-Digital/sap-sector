using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels.Components;

public record SuccessorLinkViewModel(string Name, string Url)
{
    public static IReadOnlyList<SuccessorLinkViewModel> FromSuccessors(IEnumerable<SchoolInfo> successors) =>
        successors
            .Select(successor => new SuccessorLinkViewModel(successor.Name, Routes.School(successor)))
            .ToList();
}
