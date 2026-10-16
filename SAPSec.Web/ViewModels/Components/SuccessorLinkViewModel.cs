using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels.Components;

public record SuccessorLinkViewModel(string Name, string Url)
{
    public static IReadOnlyList<SuccessorLinkViewModel> FromSuccessors(IEnumerable<SuccessorSchool> successors) =>
        successors
            .Select(successor => new SuccessorLinkViewModel(successor.Name, Routes.School(successor.Urn, successor.PhaseOfEducationName)))
            .ToList();
}
