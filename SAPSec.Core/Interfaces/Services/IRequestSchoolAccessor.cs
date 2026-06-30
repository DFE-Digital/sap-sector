using Microsoft.AspNetCore.Http;
using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services;

public interface IRequestSchoolAccessor
{
    Task<SchoolDetails> GetAsync(HttpContext? httpContext, string urn);
}
