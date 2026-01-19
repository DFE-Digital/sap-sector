using SmartBreadcrumbs.Nodes;

namespace SAPSec.Web.Constants;

public static class BreadcrumbNodes
{
    /// <summary>
    /// Creates a breadcrumb node for the school home page.
    /// </summary>
    public static BreadcrumbNode SchoolHome(string urn)
    {
        return new BreadcrumbNode
        {
            Title = "Home",
            Url = $"/school/{urn}"
        };
    }
    /// <summary>
    /// Creates a breadcrumb node for the school details page.
    /// </summary>
    public static BreadcrumbNode SchoolDetails(string urn, string schoolName)
    {
        return new BreadcrumbNode
        {
            Title = "School details",
            Url = $"/school/{urn}",
            Parent = new BreadcrumbNode
            {
                Title = "Home",
                Url = "/"
            }
        };
    }
}
/// <summary>
/// Represents a breadcrumb navigation node.
/// </summary>
public class BreadcrumbNode
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public BreadcrumbNode? Parent { get; init; }
}