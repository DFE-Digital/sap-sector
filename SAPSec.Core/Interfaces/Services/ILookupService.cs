using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services;
public interface ILookupService
{
    /// <summary>
    /// Gets all lookups
    /// </summary>
    IEnumerable<Lookup> GetAllLookups();

    /// <summary>
    /// Gets a lookup value by type and ID. Uses cached dictionary for O(1) lookup.
    /// </summary>
    /// <param name="lookupType">The lookup type</param>
    /// <param name="id">The lookup ID</param>
    /// <returns>The lookup name, or empty string if not found</returns>
    string GetLookupValue(string lookupType, string? id);

    /// <summary>
    /// Gets all lookups of a specific type
    /// </summary>
    /// <param name="lookupType">The lookup type </param>
    /// <returns>Collection of lookups for that type</returns>
    IReadOnlyList<Lookup> GetLookupsByType(string lookupType);

    /// <summary>
    /// Clears the cache (useful for testing or when data changes)
    /// </summary>
    void ClearCache();
}
