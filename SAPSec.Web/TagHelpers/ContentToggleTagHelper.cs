using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SAPSec.Web.TagHelpers;

[HtmlTargetElement("content-toggle")]
public class ContentToggleTagHelper : TagHelper
{
    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var items = new List<ContentToggleItem>();
        context.Items[ToggleContentTagHelper.ContextKey] = items;

        await output.GetChildContentAsync();

        if (items.Count < 2)
        {
            output.SuppressOutput();
            return;
        }

        var activeIndex = items.FindIndex(item => item.Active);
        if (activeIndex < 0)
        {
            activeIndex = 0;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "app-content-toggle");
        output.Attributes.SetAttribute("data-module", "app-content-toggle");

        if (!string.IsNullOrWhiteSpace(Id))
        {
            output.Attributes.SetAttribute("id", Id);
        }

        var header = new TagBuilder("div");
        header.AddCssClass("app-content-toggle__header");

        var title = new TagBuilder("h3");
        title.AddCssClass("govuk-heading-m");
        title.AddCssClass("app-content-toggle__title");
        title.InnerHtml.Append(items[activeIndex].Name);

        var button = new TagBuilder("button");
        button.Attributes["type"] = "button";
        button.AddCssClass("govuk-button");
        button.AddCssClass("govuk-button--secondary");
        button.Attributes["aria-pressed"] = activeIndex == 0 ? "false" : "true";
        button.Attributes["data-module"] = "govuk-button";
        button.InnerHtml.Append($"Show {items[(activeIndex + 1) % items.Count].Name.ToLowerInvariant()}");

        header.InnerHtml.AppendHtml(title);
        header.InnerHtml.AppendHtml(button);

        output.Content.AppendHtml(header);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var panel = new TagBuilder("div");
            panel.AddCssClass("app-content-toggle__panel");
            panel.Attributes["data-content-toggle-panel"] = "true";
            panel.Attributes["data-content-toggle-name"] = item.Name;

            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                panel.Attributes["id"] = item.Id;
            }

            if (i == activeIndex)
            {
                panel.AddCssClass("app-content-toggle__panel--active");
            }
            else
            {
                panel.Attributes["hidden"] = "hidden";
            }

            panel.InnerHtml.AppendHtml(item.Content);
            output.Content.AppendHtml(panel);
        }
    }
}
