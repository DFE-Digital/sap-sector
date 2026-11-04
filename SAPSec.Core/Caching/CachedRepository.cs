using SAPSec.Data.Repositories;
using System.Linq.Expressions;
using System.Text.Json;

namespace SAPSec.Core.Caching;

public class CachedRepository<TRepository>(ICache cache, TRepository repository)
    where TRepository : class, IRepository
{
    protected async Task<TReturnValue> CallCachedMethod<TReturnValue>(Expression<Func<TRepository, Task<TReturnValue>>> lambda)
    {
        if (lambda.Body is not MethodCallExpression methodCall)
        {
            throw new Exception("!");
        }

        var parameters = methodCall.Method.GetParameters().ToList();

        var arguments = methodCall.Arguments.Select(a => Expression.Lambda(a).Compile().DynamicInvoke()).ToList();

        if (parameters.Count != arguments.Count)
        {
            throw new Exception("!");
        }

        var dict = parameters.Select((p, i) => new { p.Name, Value = arguments[i] }).ToDictionary(t => t.Name ?? "", t => t.Value);

        var key = JsonSerializer.Serialize(dict);

        return await cache.TryGet($"Repository::{repository.GetType().Name}::{methodCall.Method.Name}::{key}", () => lambda.Compile()(repository));
    }
}