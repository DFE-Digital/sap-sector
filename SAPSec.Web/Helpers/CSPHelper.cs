using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SAPSec.Web.Helpers;

[ExcludeFromCodeCoverage]
public static class CSPHelper
{
    public static HtmlString ScriptNonce(this IHtmlHelper htmlHelper)
    {
        var context = htmlHelper.ViewContext.HttpContext;
        return context.Items["ScriptNonce"] is string scriptNonce ? new HtmlString(scriptNonce) : HtmlString.Empty;
    }
}