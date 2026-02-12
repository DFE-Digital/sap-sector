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
    /// <exception cref="KeyNotFoundException">Thrown when school is not found</exception>
    Task<SchoolDetails> GetByUrnAsync(string urn);

    /// <summary>
    /// Tries to get school details by URN.
    /// </summary>
    /// <param name="urn">The school's URN</param>
    /// <returns>School details if found, null otherwise</returns>
    Task<SchoolDetails?> TryGetByUrnAsync(string urn);

    /// <summary>
    /// Gets school details by any identifier (URN, DfE number, UKPRN).
    /// </summary>
    /// <param name="identifier">The school identifier</param>
    /// <returns>School details if found, null otherwise</returns>
    Task<SchoolDetails?> GetByIdentifierAsync(string identifier);
}