using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

public class RequestSchoolAccessor(ISchoolDetailsService schoolDetailsService) : IRequestSchoolAccessor
{
    public async Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn)
    {
        return await schoolDetailsService.GetByUrnAsync(urn);
    }
}
