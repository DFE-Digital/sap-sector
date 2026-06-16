using Microsoft.Extensions.Logging;

namespace SAPSec.Infrastructure.Json;

public class JsonFileFactory(ILoggerFactory loggerFactory) : IJsonFileFactory
{
    public IJsonFile<T> Create<T>(JsonDataSource dataSource) where T : class
    {
        return new JsonFile<T>(loggerFactory.CreateLogger<JsonFile<T>>(), dataSource);
    }
}
