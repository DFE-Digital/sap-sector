using Microsoft.AspNetCore.Mvc.Testing;

namespace SAPSec.Integration.Tests.Infrastructure;

public class WebApplicationSetupFixture : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public HttpClient NonRedirectingClient { get; private set; } = null!;

    public Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();

        if(_factory.Server == null) throw new InvalidOperationException("Test Server not started");

        // Client that follows redirects
        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = _factory.ClientOptions.BaseAddress,
            AllowAutoRedirect = true
        });

        // Client that doesn't follow redirects
        NonRedirectingClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = _factory.ClientOptions.BaseAddress,
            AllowAutoRedirect = false
        });

        Console.WriteLine($"Fixture initialized with base URL: {_factory.ClientOptions.BaseAddress}");

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        NonRedirectingClient?.Dispose();
        await _factory.DisposeAsync();
    }
}