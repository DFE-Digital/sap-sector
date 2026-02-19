namespace SAPSec.Infrastructure.Json;

public interface IJsonFile<T> where T : class
{
    Task<IEnumerable<T>> ReadAllAsync();
}