using SAPSec.Core.Exceptions;
using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Core.Features.SchoolDetails;

public interface ILinkedSchoolService
{
    /// <summary>
    /// Gets school details by URN with all business logic applied.
    /// </summary>
    /// <param name="urn">The school's URN</param>
    /// <returns>School details with data availability information</returns>
    /// <exception cref="NotFoundException">Thrown when school is not found</exception>
    Task<LinkedSchool> GetByUrnAsync(string urn);
}