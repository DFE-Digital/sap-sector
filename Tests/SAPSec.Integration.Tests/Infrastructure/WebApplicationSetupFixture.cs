using Microsoft.AspNetCore.Mvc.Testing;

namespace SAPSec.Integration.Tests.Infrastructure;

public class WebApplicationSetupFixture : IAsyncLifetime
{
    public HttpClient Client = null!;
    public HttpClient NonRedirectingClient = null!;
    private TestWebApplicationFactory _factory = null!;

    public Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();

        if(_factory.Server == null) throw new InvalidOperationException("Test Server not started");

        Client = _factory.CreateDefaultClient(_factory.ClientOptions.BaseAddress);
        NonRedirectingClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = _factory.ClientOptions.BaseAddress, AllowAutoRedirect = false });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}
