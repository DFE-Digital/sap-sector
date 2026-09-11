using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;

namespace SAPSec.Web.ViewModels;

public sealed class SchoolSideNavigationViewModel
{
    public required IReadOnlyList<SchoolSideNavigationItemViewModel> Items { get; init; }

    public static SchoolSideNavigationViewModel CreatePrimary(IUrlHelper url, string urn, string? currentAction, bool hasSimilarSchools = true, bool includeRiseResources = false)
    {
        var items = new List<SchoolSideNavigationItemViewModel>
        {
            new() { Text = "Overview", Href = Routes.PrimarySchool(urn).Overview, IsSelected = currentAction == "Index" },
            new() { Text = "KS2", Href = Routes.PrimarySchool(urn).KS2, IsSelected = currentAction == "Ks2PerformanceMeasures" },
            new() { Text = "Attendance", Href = Routes.PrimarySchool(urn).Attendance, IsSelected = currentAction == "Attendance" },
            new() { Text = "View similar schools", Href = Routes.PrimarySchool(urn).ViewSimilarSchools, IsSelected = currentAction == "ViewSimilarSchools" },
            new() { Text = "School details", Href = Routes.PrimarySchool(urn).SchoolDetails, IsSelected = currentAction == "SchoolDetails" },
            new() { Text = "What is a similar school?", Href = Routes.PrimarySchool(urn).WhatIsASimilarSchool, IsSelected = currentAction == "WhatIsASimilarSchool" }
        };

        if (!hasSimilarSchools)
        {
            items.RemoveAt(3);
        }

        if (includeRiseResources)
        {
            items.Add(new() { Text = "RISE resources", Href = Routes.PrimarySchool(urn).RiseResources, IsSelected = currentAction == "RiseResources" });
        }

        return new SchoolSideNavigationViewModel { Items = items };
    }

    public static SchoolSideNavigationViewModel CreateSecondary(IUrlHelper url, string urn, string? currentAction, bool includeRiseResources = false)
    {
        var items = new List<SchoolSideNavigationItemViewModel>
        {
            new() { Text = "Overview", Href = Routes.SecondarySchool(urn).Overview, IsSelected = currentAction == "Index" },
            new() { Text = "KS4 headline measures", Href = Routes.SecondarySchool(urn).KS4HeadlineMeasures, IsSelected = currentAction == "Ks4HeadlineMeasures" },
            new() { Text = "KS4 core subjects", Href = Routes.SecondarySchool(urn).KS4CoreSubjects, IsSelected = currentAction == "Ks4CoreSubjects" },
            new() { Text = "Attendance", Href = Routes.SecondarySchool(urn).Attendance, IsSelected = currentAction == "Attendance" },
            new() { Text = "View similar schools", Href = Routes.SecondarySchool(urn).ViewSimilarSchools, IsSelected = currentAction == "ViewSimilarSchools" },
            new() { Text = "School details", Href = Routes.SecondarySchool(urn).SchoolDetails, IsSelected = currentAction == "SchoolDetails" },
            new() { Text = "What is a similar school?", Href = Routes.SecondarySchool(urn).WhatIsASimilarSchool, IsSelected = currentAction == "WhatIsASimilarSchool" }
        };

        if (includeRiseResources)
        {
            items.Add(new() { Text = "RISE resources", Href = Routes.SecondarySchool(urn).RiseResources, IsSelected = currentAction == "RiseResources" });
        }

        return new SchoolSideNavigationViewModel { Items = items };
    }
}

public sealed class SchoolSideNavigationItemViewModel
{
    public required string Text { get; init; }
    public required string Href { get; init; }
    public bool IsSelected { get; init; }
}
