using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SAPSec.Web.TagHelpers;

/// <summary>
/// Renders the element only when the current user satisfies the named authorization policy,
/// e.g. <c>&lt;li authorize-policy="SchoolAccess"&gt;</c>.
/// </summary>
[HtmlTargetElement(Attributes = AttributeName)]
public class AuthorizePolicyTagHelper(IAuthorizationService authorizationService) : TagHelper
{
    private const string AttributeName = "authorize-policy";

    [HtmlAttributeName(AttributeName)]
    public string Policy { get; set; } = string.Empty;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var result = await authorizationService.AuthorizeAsync(ViewContext.HttpContext.User, Policy);

        if (!result.Succeeded)
        {
            output.SuppressOutput();
        }
    }
}
