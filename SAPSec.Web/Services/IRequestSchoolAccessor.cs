using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.Services;

public interface IRequestSchoolAccessor
{
    Task<LinkedSchool> GetAsync(HttpContext? httpContext, string urn);
}
