using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.ViewModels;

public sealed class SchoolSideNavigationViewModel
{
    public required IReadOnlyList<SchoolSideNavigationItemViewModel> Items { get; init; }

    public static SchoolSideNavigationViewModel CreatePrimary(IUrlHelper url, string urn, string? currentAction) =>
        new()
        {
            Items =
            [
                new() { Text = "Overview", Href = url.Action("Index", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "Index" },
                new() { Text = "KS2", Href = url.Action("Ks2", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "Ks2" },
                new() { Text = "Attendance", Href = url.Action("Attendance", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "Attendance" },
                new() { Text = "View similar schools", Href = url.Action("ViewSimilarSchools", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "ViewSimilarSchools" },
                new() { Text = "School details", Href = url.Action("SchoolDetails", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "SchoolDetails" },
                new() { Text = "What is a similar school?", Href = url.Action("WhatIsASimilarSchool", "School", new { area = "Primary", urn })!, IsSelected = currentAction == "WhatIsASimilarSchool" }
            ]
        };

    public static SchoolSideNavigationViewModel CreateSecondary(IUrlHelper url, string urn, string? currentAction) =>
        new()
        {
            Items =
            [
                new() { Text = "Overview", Href = url.Action("Index", "School", new { urn })!, IsSelected = currentAction == "Index" },
                new() { Text = "KS4 headline measures", Href = url.Action("Ks4HeadlineMeasures", "School", new { urn })!, IsSelected = currentAction == "Ks4HeadlineMeasures" },
                new() { Text = "KS4 core subjects", Href = url.Action("Ks4CoreSubjects", "School", new { urn })!, IsSelected = currentAction == "Ks4CoreSubjects" },
                new() { Text = "Attendance", Href = url.Action("Attendance", "School", new { urn })!, IsSelected = currentAction == "Attendance" },
                new() { Text = "View similar schools", Href = url.Action("ViewSimilarSchools", "SimilarSchools", new { urn })!, IsSelected = currentAction == "ViewSimilarSchools" },
                new() { Text = "School details", Href = url.Action("SchoolDetails", "School", new { urn })!, IsSelected = currentAction == "SchoolDetails" },
                new() { Text = "What is a similar school?", Href = url.Action("WhatIsASimilarSchool", "School", new { urn })!, IsSelected = currentAction == "WhatIsASimilarSchool" }
            ]
        };
}

public sealed class SchoolSideNavigationItemViewModel
{
    public required string Text { get; init; }
    public required string Href { get; init; }
    public bool IsSelected { get; init; }
}
