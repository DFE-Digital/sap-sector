namespace SAPSec.Web.Constants;

public static class Routes
{
    public const string Home = "/";
    public const string SignIn = "/auth/signin";
    public const string FindASchool = "/find-a-school";
    public static string School(string urn) => $"/school/{urn}";
    public const string Error = "/error";
    public const string AccessDenied = "/error/403";
}
