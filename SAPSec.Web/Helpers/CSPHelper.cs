using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace SAPSec.Web.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class CSPHelper
    {
        public static string RandomCharacters => GenerateRandom();

        private static string GenerateRandom()
        {
            var byteArray = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(byteArray);
            }

            return Convert.ToBase64String(byteArray);
        }

        public static HtmlString ScriptNonce(this IHtmlHelper htmlHelper)
        {
            var context = htmlHelper.ViewContext.HttpContext;
            return context.Items["ScriptNonce"] is string scriptNonce ? new HtmlString(scriptNonce) : HtmlString.Empty;
        }
    }
}
