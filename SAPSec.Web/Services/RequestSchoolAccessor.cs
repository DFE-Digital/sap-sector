using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Services;

public class RequestSchoolAccessor(ILinkedSchoolService service) : IRequestSchoolAccessor
{
    private const string HttpContextItemsKeyPrefix = "RequestSchoolAccessor:SchoolDetails:";

    public async Task<LinkedSchool> GetAsync(HttpContext? httpContext, string urn)
    {
        if (httpContext?.Items is not null &&
            httpContext.Items.TryGetValue(BuildCacheKey(urn), out var cachedSchool) &&
            cachedSchool is LinkedSchool linkedSchool)
        {
            return linkedSchool;
        }

        var school = await service.GetByUrnAsync(urn);

        if (httpContext?.Items is not null)
        {
            httpContext.Items[BuildCacheKey(urn)] = school;
        }

        return school;
    }

    private static string BuildCacheKey(string urn) => HttpContextItemsKeyPrefix + urn;
}
