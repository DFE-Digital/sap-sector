namespace SAPSec.Core.Constants;

/// <summary>
/// Constants for external service URLs.
/// Centralises all external service URL templates.
/// </summary>
public static class ExternalServiceUrls
{
    /// <summary>Ofsted reports base URL</summary>
    public const string OfstedReportsBase = "https://reports.ofsted.gov.uk/provider/23";

    /// <summary>Financial Benchmarking and Insights Tool base URL</summary>
    public const string FinancialBenchmarkingBase = "https://financial-benchmarking-and-insights-tool.education.gov.uk/school";

    /// <summary>GIAS establishment details base URL</summary>
    public const string GiasEstablishmentBase = "https://get-information-schools.service.gov.uk/Establishments/Establishment/Details";

    /// <summary>GIAS trust/group details base URL</summary>
    public const string GiasTrustBase = "https://get-information-schools.service.gov.uk/Groups/Group/Details";

    /// <summary>GIAS home page</summary>
    public const string GiasHome = "https://get-information-schools.service.gov.uk";

    /// <summary>Explore Education Statistics home page</summary>
    public const string ExploreEducationStatistics = "https://explore-education-statistics.service.gov.uk";

    /// <summary>Gets the Ofsted report URL for a school</summary>
    public static string GetOfstedReportUrl(string urn)
        => $"{OfstedReportsBase}/{urn}";

    /// <summary>Gets the Financial Benchmarking URL for a school</summary>
    public static string GetFinancialBenchmarkingUrl(string urn)
        => $"{FinancialBenchmarkingBase}/{urn}";

    /// <summary>Gets the GIAS establishment details URL for a school</summary>
    public static string GetGiasEstablishmentUrl(string urn)
        => $"{GiasEstablishmentBase}/{urn}";

    /// <summary>Gets the GIAS trust details URL</summary>
    public static string GetGiasTrustUrl(string trustId)
        => $"{GiasTrustBase}/{trustId}";
}