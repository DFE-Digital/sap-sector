using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SAPSec.Web.TagHelpers;

[HtmlTargetElement("tabbed-view")]
public class TabbedViewTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public string? HtmlPrefix { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tabContext = new TabbedViewContext();
        tabContext.HtmlPrefix = HtmlPrefix;
        context.Items.Add(typeof(TabbedViewTagHelper), tabContext);

        await output.GetChildContentAsync();

        output.Content.AppendFormat(
            """
            <div class="govuk-tabs govuk-!-margin-bottom-3 app-ks4-tabs" data-module="govuk-tabs">
                <ul class="govuk-tabs__list">
            """);


        foreach (var (tab, i) in tabContext.Tabs.Select((t, i) => (t, i)))
        {
            var selected = i == 0 ? "govuk-tabs__list-item--selected" : "";
            output.Content.AppendFormat(
                """
                    <li class="govuk-tabs__list-item {3}"><a class="govuk-tabs__tab" href="#{0}-{1}">{2}</a></li>
                """, HtmlPrefix, tab.Id, tab.Name, selected);
        }

        output.Content.AppendHtml("</ul>");

        foreach (var tab in tabContext.Tabs)
        {
            output.Content.AppendFormat(
                """
                <div class="govuk-tabs__panel" id="{0}-{1}">
            """, HtmlPrefix, tab.Id);

            output.Content.AppendHtml(tab.Content);

            output.Content.AppendHtml("</div>");
        }

        output.Content.AppendHtml("</div>");
    }
}

public class TabbedViewContext
{
    public string? HtmlPrefix { get; set; }
    public List<Tab> Tabs { get; set; } = [];
}

public record Tab(string? Id, string? Name, IHtmlContent Content);

[HtmlTargetElement("tab-content")]
public class TabbedContentTagHelper : TagHelper
{
    public string? Id { get; set; }
    public string? Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        var tabContext = (TabbedViewContext)context.Items[typeof(TabbedViewTagHelper)];

        tabContext.Tabs.Add(new(Id, Name, content));

        output.SuppressOutput();
    }
}