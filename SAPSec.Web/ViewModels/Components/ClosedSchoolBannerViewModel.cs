namespace SAPSec.Web.ViewModels.Components;

public enum ClosedSchoolBannerVariant
{
    ThisSchool,
    Comparator
}

public record ClosedSchoolBannerViewModel(ClosedSchoolBannerVariant Variant, IReadOnlyList<SuccessorLinkViewModel> Successors)
{
    public bool HasSuccessors => Successors.Count > 0;

    public string SuccessorNamesJoined => Successors.Count switch
    {
        0 => string.Empty,
        1 => Successors[0].Name,
        _ => string.Join(", ", Successors.Take(Successors.Count - 1).Select(s => s.Name)) + " and " + Successors[^1].Name
    };
}
