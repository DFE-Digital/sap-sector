using Microsoft.AspNetCore.Http;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Areas.AllThrough.Services;

public interface IAllThroughSimilarSchoolsTabUrlBuilder
{
    AllThroughSimilarSchoolsTabUrls Build(string urn, string selectedPhase, IQueryCollection query);
}

public class AllThroughSimilarSchoolsTabUrlBuilder : IAllThroughSimilarSchoolsTabUrlBuilder
{
    public AllThroughSimilarSchoolsTabUrls Build(string urn, string selectedPhase, IQueryCollection query)
    {
        var primaryQuery = query.TryGetValue("primaryQuery", out var primaryValue)
            ? primaryValue.FirstOrDefault()
            : null;
        var secondaryQuery = query.TryGetValue("secondaryQuery", out var secondaryValue)
            ? secondaryValue.FirstOrDefault()
            : null;

        return new(
            BuildPrimaryTabUrl(urn, selectedPhase, query, primaryQuery, secondaryQuery),
            BuildSecondaryTabUrl(urn, selectedPhase, query, secondaryQuery));
    }

    private string BuildPrimaryTabUrl(
        string urn,
        string selectedPhase,
        IQueryCollection query,
        string? primaryQuery,
        string? secondaryQuery)
    {
        var url = Routes.AllThroughSchool(urn).ViewSimilarSchools;
        if (!string.IsNullOrWhiteSpace(primaryQuery) && primaryQuery.StartsWith('?'))
        {
            url += primaryQuery;
        }
        else if (selectedPhase == "primary")
        {
            url += BuildPhaseQueryString(query);
        }

        var secondaryQueryString = selectedPhase == "secondary"
            ? BuildPhaseQueryString(query)
            : secondaryQuery;

        return AppendStoredQuery(url, "secondaryQuery", secondaryQueryString);
    }

    private string BuildSecondaryTabUrl(
        string urn,
        string selectedPhase,
        IQueryCollection query,
        string? secondaryQuery)
    {
        var url = $"{Routes.AllThroughSchool(urn).ViewSimilarSchools}?phase=secondary";
        var secondaryQueryString = selectedPhase == "secondary"
            ? BuildPhaseQueryString(query)
            : secondaryQuery;

        if (!string.IsNullOrWhiteSpace(secondaryQueryString) && secondaryQueryString.StartsWith('?'))
        {
            url += "&" + secondaryQueryString.TrimStart('?');
        }

        var primaryQueryString = selectedPhase == "primary"
            ? BuildPhaseQueryString(query)
            : query.TryGetValue("primaryQuery", out var primaryValue)
                ? primaryValue.FirstOrDefault()
                : null;

        return AppendStoredQuery(url, "primaryQuery", primaryQueryString);
    }

    private static string BuildPhaseQueryString(IQueryCollection query)
    {
        var queryParts = new List<string>();

        foreach (var (key, values) in query)
        {
            if (key.Equals("phase", StringComparison.InvariantCultureIgnoreCase)
                || key.Equals("primaryQuery", StringComparison.InvariantCultureIgnoreCase)
                || key.Equals("secondaryQuery", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value!)}");
                }
            }
        }

        return queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;
    }

    private static string AppendStoredQuery(string url, string key, string? queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return url;
        }

        var separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}{key}={Uri.EscapeDataString(queryString)}";
    }
}

public record AllThroughSimilarSchoolsTabUrls(
    string PrimaryTabUrl,
    string SecondaryTabUrl);
