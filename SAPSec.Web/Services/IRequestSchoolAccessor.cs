using SAPSec.Core.School.Details;

namespace SAPSec.Web.Services;

public interface IRequestSchoolAccessor
{
    Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn);
}
