using Xunit;

namespace SAPSec.UI.Tests.Infrastructure;

public class WebApplicationSetupFixture : IAsyncLifetime
{
    private TestWebApplicationFactory? _factory;

    public string BaseUrl { get; private set; } = null!;

    public Task InitializeAsync()
    {
        // Spin up the test server using TestWebApplicationFactory
        _factory = new TestWebApplicationFactory();

        if(_factory.Server == null) throw new InvalidOperationException("Test Server not started");

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
