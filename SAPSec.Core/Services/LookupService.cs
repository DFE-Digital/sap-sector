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
    private readonly Lazy<Dictionary<(string Type, string Id), string>> _lookupCache;

    public LookupService(
        ILookupRepository lookupRepository,
        ILogger<LookupService> logger)
    {
        _lookupRepository = lookupRepository;
        _logger = logger;
        _lookupCache = new Lazy<Dictionary<(string Type, string Id), string>>(
            LoadCache,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets a lookup value by type and ID. Uses cached dictionary for O(1) lookup.
    /// </summary>
    public string GetLookupValue(string lookupType, string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;

        return _lookupCache.Value.TryGetValue((lookupType, id), out var name)
            ? name
            : string.Empty;
    }

    private Dictionary<(string Type, string Id), string> LoadCache()
    {
        _logger.LogInformation("Loading lookup cache...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var lookups = _lookupRepository.GetAllLookups();

        var cache = lookups
            .Where(l => !string.IsNullOrWhiteSpace(l.LookupType) && !string.IsNullOrWhiteSpace(l.Id))
            .ToDictionary(
                l => (l.LookupType, l.Id),
                l => l.Name ?? string.Empty);

        stopwatch.Stop();
        _logger.LogInformation(
            "Lookup cache loaded: {Count} lookups in {ElapsedMs}ms",
            cache.Count,
            stopwatch.ElapsedMilliseconds);

        return cache;
    }
}