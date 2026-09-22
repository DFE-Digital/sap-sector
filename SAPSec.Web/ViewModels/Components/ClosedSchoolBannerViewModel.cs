namespace SAPSec.Web.ViewModels.Components;

public enum ClosedSchoolBannerVariant
{
    ThisSchool,
    Comparator
}

public record ClosedSchoolBannerViewModel(
    ClosedSchoolBannerVariant Variant,
    IReadOnlyList<SuccessorLinkViewModel> Successors,
    IReadOnlyList<SuccessorLinkViewModel> Predecessors)
{
    public bool HasSuccessors => Successors.Count > 0;
    public bool HasPredecessors => Predecessors.Count > 0;

    public string SuccessorNamesJoined => JoinNames(Successors);
    public string PredecessorNamesJoined => JoinNames(Predecessors);

    private static string JoinNames(IReadOnlyList<SuccessorLinkViewModel> schools) => schools.Count switch
    {
        0 => string.Empty,
        1 => schools[0].Name,
        _ => string.Join(", ", schools.Take(schools.Count - 1).Select(s => s.Name)) + " and " + schools[^1].Name
    };
}
