using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

public class RequestSchoolAccessor(ISchoolDetailsService schoolDetailsService) : IRequestSchoolAccessor
{
    private const string HttpContextItemsKeyPrefix = "RequestSchoolAccessor:SchoolDetails:";

    public async Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn)
    {
        if (httpContext?.Items is not null &&
            httpContext.Items.TryGetValue(BuildCacheKey(urn), out var cachedSchool) &&
            cachedSchool is SchoolDetails schoolDetails)
        {
            return schoolDetails;
        }

        var school = await schoolDetailsService.GetByUrnAsync(urn);

        if (httpContext?.Items is not null)
        {
            httpContext.Items[BuildCacheKey(urn)] = school;
        }

        return school;
    }

    private static string BuildCacheKey(string urn) => HttpContextItemsKeyPrefix + urn;
}
