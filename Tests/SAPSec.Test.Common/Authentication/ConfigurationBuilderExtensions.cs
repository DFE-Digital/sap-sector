using Microsoft.Extensions.Configuration;

namespace SAPSec.Test.Common.Authentication;

public static class ConfigurationBuilderExtensions
{
    private static class TestValues
    {
        // DSI Test Values
        public const string ClientId = "test-client-id";
        public const string ClientSecret = "test-client-secret";
        public const string Authority = "https://test-oidc.signin.education.gov.uk";
        public const string ApiUri = "https://test-api.signin.education.gov.uk";
        public const string ApiSecret = "test-api-secret";
        public const string Audience = "test-audience";
        public const string TokenExpiryMinutes = "60";
    }

    public static IConfigurationBuilder AddTestDsiConfiguration(this IConfigurationBuilder builder)
    {
        var configurationValues = new Dictionary<string, string?>
        {
            { "DsiConfiguration:ClientId", TestValues.ClientId },
            { "DsiConfiguration:ClientSecret", TestValues.ClientSecret },
            { "DsiConfiguration:Authority", TestValues.Authority },
            { "DsiConfiguration:RequireHttpsMetadata", "false" },
            { "DsiConfiguration:ValidateIssuer", "false" },
            { "DsiConfiguration:ValidateAudience", "false" },
            { "DsiConfiguration:ApiUri", TestValues.ApiUri },
            { "DsiConfiguration:ApiSecret", TestValues.ApiSecret },
            { "DsiConfiguration:Audience", TestValues.Audience },
            { "DsiConfiguration:TokenExpiryMinutes", TestValues.TokenExpiryMinutes }
        };

        builder.AddInMemoryCollection(configurationValues);

        return builder;
    }
}