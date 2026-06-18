using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SAPSec.Data.Json;

public class JsonFile<T> : IJsonFile<T> where T : class
{
    private readonly string _filePath;
    private readonly ILogger<JsonFile<T>> _logger;
    private readonly Lazy<List<T>> _cache;

    public JsonFile(ILogger<JsonFile<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException();
        var basePath = AppContext.BaseDirectory;
        _filePath = Path.Combine(basePath, "Files", "Generated");
        _cache = new Lazy<List<T>>(
            LoadCache,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<IEnumerable<T>> ReadAllAsync()
    {
        return Task.FromResult(_cache.Value.AsEnumerable());
    }

    private string ReadFile(string fileName)
    {
        var fullPath = Path.Combine(_filePath, $"{fileName}.json");

        return File.ReadAllText(fullPath);
    }

    private List<T> LoadCache()
    {
        _logger.LogInformation("Loading lookup cache...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var fileData = ReadFile(typeof(T).Name);
        if (string.IsNullOrWhiteSpace(fileData))
        {
            return [];
        }

        return JsonConvert.DeserializeObject<List<T>>(fileData) ?? [];
    }
}
