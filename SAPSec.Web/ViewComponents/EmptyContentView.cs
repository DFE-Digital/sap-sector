using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace SAPSec.Web.ViewComponents;

public class EmptyContentView() : HtmlContentViewComponentResult(new HtmlString(string.Empty));