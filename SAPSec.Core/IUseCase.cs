namespace SAPSec.Core;

public interface IUseCase<TRequest, TResponse>
{
    Task<TResponse> Execute(TRequest request);
}
