using Microsoft.AspNetCore.Http;

namespace SAPSec.Web.Extensions;

public static class HttpRequestExtensions
{
    public static string GetCanonicalUrl(this HttpRequest request) =>
        $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
}
