namespace SAPSec.Infrastructure.Json;

public interface IJsonFileFactory
{
    IJsonFile<T> Create<T>(JsonDataSource dataSource) where T : class;
}
