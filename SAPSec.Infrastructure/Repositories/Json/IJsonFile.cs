namespace SAPSec.Infrastructure.Repositories.Json;

public interface IJsonFile<T> where T : class
{
    Task<IEnumerable<T>> ReadAllAsync();
}