using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SAPSec.Web.TagHelpers;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Authorization;

public class AuthorizePolicyTagHelperTests
{
    [Fact]
    public async Task WhenPolicySucceeds_RendersElement()
    {
        var output = await ProcessAsync(succeeds: true);

        output.TagName.Should().Be("li");
        output.Content.IsEmptyOrWhiteSpace.Should().BeTrue();
    }

    [Fact]
    public async Task WhenPolicyFails_SuppressesElement()
    {
        var output = await ProcessAsync(succeeds: false);

        output.TagName.Should().BeNull();
        output.Content.GetContent().Should().BeEmpty();
    }

    private static async Task<TagHelperOutput> ProcessAsync(bool succeeds)
    {
        var tagHelper = new AuthorizePolicyTagHelper(new StubAuthorizationService(succeeds))
        {
            Policy = "SomePolicy",
            ViewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("TestAuth")) }
            }
        };

        var context = new TagHelperContext([], new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput(
            "li",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(context, output);
        return output;
    }

    private sealed class StubAuthorizationService(bool succeeds) : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
            Task.FromResult(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed());
    }
}
