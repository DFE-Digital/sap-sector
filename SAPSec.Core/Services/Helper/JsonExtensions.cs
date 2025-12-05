using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.Helper;

public static class JsonExtensions
{
    public static List<T> DeserializeToList<T>(this string? json, JsonSerializerOptions? options = null, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        var trimmed = json.Trim();

        try
        {
            if (trimmed.StartsWith('['))
            {
                return JsonSerializer.Deserialize<List<T>>(trimmed, options) ?? new List<T>();
            }

            if (trimmed.StartsWith('{'))
            {
                var item = JsonSerializer.Deserialize<T>(trimmed, options);
                return item != null ? new List<T> { item } : new List<T>();
            }

            logger?.LogWarning("JSON does not start with '[' or '{{': {Json}",
                trimmed.Length > 100 ? trimmed[..100] + "..." : trimmed);

            return new List<T>();
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to deserialize JSON to {Type}", typeof(T).Name);
            return new List<T>();
        }
    }
}
