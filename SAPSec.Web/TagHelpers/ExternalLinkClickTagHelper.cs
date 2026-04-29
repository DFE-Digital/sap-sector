using Dfe.Analytics.AspNetCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SAPSec.Web.TagHelpers;

[HtmlTargetElement("a", Attributes = "href")]
public class ExternalLinkClickTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        //  var attributes = context.AllAttributes["add-nonce"];

        var href = output.Attributes["href"]?.Value.ToString();

        if (href.StartsWith("http"))
        {
            //output.Attributes.SetAttribute("rel", "nofollow");

            ViewContext.HttpContext.GetWebRequestEvent()?.AddData("External link click", "external link clicked!!!");
        }
        //if (bool.TryParse(attributes.Value.ToString(), out var shouldAdd) && shouldAdd)
        //{
        //    output.Attributes.Remove(attributes);
        //    output.Attributes.Add(new TagHelperAttribute("nonce", ViewContext?.HttpContext.Items["ScriptNonce"]));
        //}
    }
}