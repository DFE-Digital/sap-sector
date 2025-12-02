using Microsoft.AspNetCore.Mvc.Testing;

namespace SAPSec.Integration.Tests.Infrastructure;

public class WebApplicationSetupFixture : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;

    public TestWebApplicationFactory Factory => _factory;

    /// <summary>
    /// Client that follows redirects - authenticated via "Testing" environment
    /// </summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Client that doesn't follow redirects - authenticated via "Testing" environment
    /// </summary>
    public HttpClient NonRedirectingClient { get; private set; } = null!;

    /// <summary>
    /// Alias for NonRedirectingClient - for backward compatibility with tests using AuthenticatedClient
    /// </summary>
    public HttpClient AuthenticatedClient => NonRedirectingClient;

    public Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();

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

        Console.WriteLine($"✅ Fixture initialized with base URL: {_factory.ClientOptions.BaseAddress}");

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        NonRedirectingClient?.Dispose();
        await _factory.DisposeAsync();
    }
}