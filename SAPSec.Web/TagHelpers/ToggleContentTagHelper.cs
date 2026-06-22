using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SAPSec.Web.TagHelpers;

[HtmlTargetElement("toggle-content", ParentTag = "content-toggle")]
public class ToggleContentTagHelper : TagHelper
{
    internal static readonly object ContextKey = new();

    [HtmlAttributeName("id")]
    public string Id { get; set; } = string.Empty;

    [HtmlAttributeName("name")]
    public string Name { get; set; } = string.Empty;

    [HtmlAttributeName("active")]
    public bool Active { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context.Items.TryGetValue(ContextKey, out var items)
            && items is IList<ContentToggleItem> toggleItems)
        {
            var childContent = await output.GetChildContentAsync();
            toggleItems.Add(new ContentToggleItem(Id, Name, new HtmlString(childContent.GetContent()), Active));
        }

        output.SuppressOutput();
    }
}
