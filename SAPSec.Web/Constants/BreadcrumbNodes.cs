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
}