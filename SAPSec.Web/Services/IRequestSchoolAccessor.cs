using SAPSec.Core.Features.SchoolDetails;

namespace SAPSec.Web.Services;

public interface IRequestSchoolAccessor
{
    Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn);
}
