using SAPSec.Test.Accessibility.Setup;
using Xunit;

namespace SAPSec.Test.EndToEnd.Setup;

public class AccessibilityTestsFixture : IAsyncLifetime
{
    private AccessibilityTestsWebApplicationFactory? _factory;

    public string BaseUrl { get; private set; } = null!;

    public Task InitializeAsync()
    {
        _factory = new AccessibilityTestsWebApplicationFactory();

        if (_factory.Server == null) throw new InvalidOperationException("Test Server not started");

        BaseUrl = _factory.ClientOptions.BaseAddress.ToString();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }
}