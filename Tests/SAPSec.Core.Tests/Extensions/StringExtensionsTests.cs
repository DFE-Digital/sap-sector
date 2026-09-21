using SAPSec.Core.Extensions;

namespace SAPSec.Core.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("/signin-oidc", "/signin-oidc")]
    [InlineData("/signin-oidc\r\nFake-Log-Entry: injected", "/signin-oidcFake-Log-Entry: injected")]
    [InlineData("line1\nline2\rline3", "line1line2line3")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void SanitizeForLog_StripsCarriageReturnsAndNewlines(string? input, string expected)
    {
        input.SanitizeForLog().Should().Be(expected);
    }
}
