using Microsoft.AspNetCore.Mvc.Testing;

namespace SAPSec.Integration.Tests.Infrastructure;

public class WebApplicationSetupFixture : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;
    private TestWebApplicationFactory _authenticatedFactory = null!;

    public TestWebApplicationFactory Factory => _factory;

    public HttpClient Client { get; private set; } = null!;
    public HttpClient NonRedirectingClient { get; private set; } = null!;
    public HttpClient AuthenticatedClient { get; private set; } = null!;

    public Task InitializeAsync()
    {
        // Factory WITHOUT authentication
        _factory = new TestWebApplicationFactory();

        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });

        NonRedirectingClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // ✅ SEPARATE factory WITH test authentication
        _authenticatedFactory = new TestWebApplicationFactory()
            .WithTestAuthentication(
                userId: "test-user-123",
                email: "test@example.com",
                organisationId: "org-123",
                organisationName: "Test Organisation"
            );

        AuthenticatedClient = _authenticatedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        NonRedirectingClient?.Dispose();
        AuthenticatedClient?.Dispose();

        await _factory.DisposeAsync();
        await _authenticatedFactory.DisposeAsync();
    }
}