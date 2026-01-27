using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Collections.Concurrent;

namespace SAPSec.Core.Services;

/// <summary>
/// Cached lookup service that loads lookups once and provides O(1) access
/// </summary>
public class LookupService : ILookupService
{
    private readonly ILookupRepository _lookupRepository;
    private readonly ILogger<LookupService> _logger;

    private readonly object _cacheLock = new();
    private volatile bool _isCacheLoaded;

    private readonly ConcurrentDictionary<(string Type, string Id), string> _lookupDictionary = new();

    private readonly ConcurrentDictionary<string, List<Lookup>> _lookupsByType = new();

    private List<Lookup> _allLookups = new();

    public LookupService(
        ILookupRepository lookupRepository,
        ILogger<LookupService> logger)
    {
        _lookupRepository = lookupRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets a lookup value by type and ID. Uses cached dictionary for O(1) lookup.
    /// </summary>
    public string GetLookupValue(string lookupType, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;

        EnsureCacheLoaded();

        return _lookupDictionary.TryGetValue((lookupType, id), out var name)
            ? name
            : string.Empty;
    }

    /// <summary>
    /// Gets all lookups of a specific type
    /// </summary>
    public IReadOnlyList<Lookup> GetLookupsByType(string lookupType)
    {
        EnsureCacheLoaded();

        return _lookupsByType.TryGetValue(lookupType, out var lookups)
            ? lookups.AsReadOnly()
            : Array.Empty<Lookup>();
    }

    /// <summary>
    /// Gets all lookups (backwards compatibility - prefer GetLookupValue for performance)
    /// </summary>
    public IEnumerable<Lookup> GetAllLookups()
    {
        EnsureCacheLoaded();
        return _allLookups;
    }

    /// <summary>
    /// Clears the cache (useful for testing or when data changes)
    /// </summary>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _lookupDictionary.Clear();
            _lookupsByType.Clear();
            _allLookups.Clear();
            _isCacheLoaded = false;
            _logger.LogInformation("Lookup cache cleared");
        }
    }

    private void EnsureCacheLoaded()
    {
        if (_isCacheLoaded)
            return;

        lock (_cacheLock)
        {
            if (_isCacheLoaded)
                return;

            LoadCache();
            _isCacheLoaded = true;
        }
    }

    private void LoadCache()
    {
        try
        {
            _logger.LogInformation("Loading lookup cache...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var lookups = _lookupRepository.GetAllLookups().ToList();
            _allLookups = lookups;

            foreach (var lookup in lookups)
            {
                if (string.IsNullOrWhiteSpace(lookup.LookupType) || string.IsNullOrWhiteSpace(lookup.Id))
                    continue;

                _lookupDictionary[(lookup.LookupType, lookup.Id)] = lookup.Name ?? string.Empty;

                if (!_lookupsByType.ContainsKey(lookup.LookupType))
                {
                    _lookupsByType[lookup.LookupType] = new List<Lookup>();
                }
                _lookupsByType[lookup.LookupType].Add(lookup);
            }

            stopwatch.Stop();
            _logger.LogInformation(
                "Lookup cache loaded: {Count} lookups, {TypeCount} types in {ElapsedMs}ms",
                lookups.Count,
                _lookupsByType.Count,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lookup cache");
            throw;
        }
    }
}