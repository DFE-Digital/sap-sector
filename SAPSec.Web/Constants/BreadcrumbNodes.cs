using SmartBreadcrumbs.Nodes;

namespace SAPSec.Web.Constants;

public static class BreadcrumbNodes
{
    public static MvcBreadcrumbNode SchoolHome(string? urn) => new("Index", "School", PageTitles.SchoolHome)
    {
        RouteValues = new
        {
            urn
        }
    };

    // public static MvcBreadcrumbNode SchoolComparison(string urn) => new("Index", "SchoolComparison", PageTitles.Comparison)
    // {
    //     RouteValues = new
    //     {
    //         urn
    //     },
    //     Parent = SchoolHome(urn)
    // };
}