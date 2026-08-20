using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SAPSec.Test.Common;

public class TestOutputLogger<T>(ITestOutputHelper output) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        output.WriteLine($"{DateTime.UtcNow.TimeOfDay} {logLevel}: {formatter(state, exception)}");
    }
}
