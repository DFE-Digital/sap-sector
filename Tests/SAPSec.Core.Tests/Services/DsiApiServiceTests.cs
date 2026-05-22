using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SAPSec.Core.Configuration;
using SAPSec.Core.Services;

namespace SAPSec.Core.Tests.Services;

public class DsiApiServiceTests
{
    [Fact]
    public async Task GenerateBearerToken_ReturnsHs256TokenSignedWithApiSecret()
    {
        var config = new DsiConfiguration
        {
            ApiUri = "https://test-api.signin.education.gov.uk",
            ClientId = "test-client-id",
            ClientSecret = "different-client-secret",
            ApiSecret = "jean-namer-weigher-manege",
            Audience = "signin.education.gov.uk",
            TokenExpiryMinutes = 60
        };

        var service = new DsiApiService(
            new HttpClient(),
            Options.Create(config),
            new Mock<ILogger<DsiApiService>>().Object);

        var token = await service.GenerateBearerToken();

        var parts = token.Split('.');
        parts.Should().HaveCount(3);

        var header = JsonDocument.Parse(Base64UrlEncoder.Decode(parts[0]));
        var payload = JsonDocument.Parse(Base64UrlEncoder.Decode(parts[1]));

        header.RootElement.GetProperty("alg").GetString().Should().Be("HS256");
        header.RootElement.GetProperty("typ").GetString().Should().Be("JWT");

        payload.RootElement.GetProperty("iss").GetString().Should().Be(config.ClientId);
        payload.RootElement.GetProperty("aud").GetString().Should().Be(config.Audience);

        var expUnix = payload.RootElement.GetProperty("exp").GetInt64();
        var exp = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        exp.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(55));
        exp.Should().BeBefore(DateTimeOffset.UtcNow.AddMinutes(65));

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Base64UrlEncoder.Encode(
            new HMACSHA256(Encoding.ASCII.GetBytes(config.ApiSecret))
                .ComputeHash(Encoding.ASCII.GetBytes(unsignedToken)));
        var clientSecretSignature = Base64UrlEncoder.Encode(
            new HMACSHA256(Encoding.ASCII.GetBytes(config.ClientSecret))
                .ComputeHash(Encoding.ASCII.GetBytes(unsignedToken)));

        parts[2].Should().Be(expectedSignature);
        parts[2].Should().NotBe(clientSecretSignature);
    }
}
