using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SAPSec.Infrastructure.Repositories.Json
{
    public class JsonFile<T> : IJsonFile<T> where T : class
    {
        private readonly string _filePath;
        private readonly ILogger<JsonFile<T>> _logger;
        private readonly Lazy<List<T>> _cache;

        public JsonFile(ILogger<JsonFile<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException();
            var basePath = AppContext.BaseDirectory;
            _filePath = Path.Combine(basePath, "Data", "Files");
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
            try
            {
                var fullPath = Path.Combine(_filePath, $"{fileName}.json");

                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Read file {fileName}! - {ex.Message}, {ex}");
                return string.Empty;
            }
        }

        private List<T> LoadCache()
        {
            _logger.LogInformation("Loading lookup cache...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var fileData = ReadFile(typeof(T).Name);
                if (!string.IsNullOrWhiteSpace(fileData))
                {
                    return JsonConvert.DeserializeObject<List<T>>(fileData) ?? [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute generic readall for {typeof(T).Name}! - {ex.Message}, {ex}");
            }

            return [];
        }
    }
}
