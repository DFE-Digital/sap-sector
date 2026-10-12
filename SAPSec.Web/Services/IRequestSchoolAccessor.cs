using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Services;

public interface IRequestSchoolAccessor
{
    Task<SchoolInfoWithLinks> GetAsync(HttpContext? httpContext, string urn);
}
