using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Filters;

namespace SAPSec.Web.Services;

public class RequestSchoolAccessor(ISchoolDetailsService schoolDetailsService) : IRequestSchoolAccessor
{
    public async Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn)
    {
        if (ValidatedSchoolDetailsStore.TryGet(httpContext, urn, out var school))
        {
            return school;
        }

        school = await schoolDetailsService.GetByUrnAsync(urn);
        ValidatedSchoolDetailsStore.Set(httpContext, urn, school);
        return school;
    }
}
