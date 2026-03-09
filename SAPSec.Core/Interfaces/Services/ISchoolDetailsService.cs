using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services;

/// <summary>
/// Service for retrieving school details with business logic applied.
/// </summary>
public interface ISchoolDetailsService
{
    /// <summary>
    /// Gets school details by URN with all business logic applied.
    /// </summary>
    /// <param name="urn">The school's URN</param>
    /// <returns>School details with data availability information</returns>
    /// <exception cref="NotFoundException">Thrown when school is not found</exception>
    Task<SchoolDetails> GetByUrnAsync(string urn);
}