using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using SAPSec.Web.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SAPSec.Web;

public class Program
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGovUkFrontend(options =>
        {
            options.Rebrand = true;
        });

        builder.Services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                options.ViewLocationFormats.Add("/{0}.cshtml");
            });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-GB");
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB") };
            options.RequestCultureProviders.Clear();
        });

        builder.Services.AddMvc();
        builder.Services.AddHealthChecks();
        builder.Services.AddDataProtection()
               .PersistKeysToFileSystem(new DirectoryInfo(@"/keys"))
               .SetApplicationName("SAPSec");

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 1 });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseGovUkFrontend();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Expect-CT", "max-age=86400, enforce");
            context.Response.Headers.Append("Referrer-Policy", "same-origin");
            context.Response.Headers.Append("Arr-Disable-Session-Affinity", "true");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
            context.Response.Headers.Append("X-XSS-Protection", "0");
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000;includeSubDomains; preload");

            context.Items["ScriptNonce"] = CSPHelper.RandomCharacters;

            context.Response.Headers.Append(
                "Content-Security-Policy",
                "base-uri 'self';"
                + "object-src 'none';"
                + "default-src 'self';"
                + "frame-ancestors 'none';"
                + "connect-src 'self' *.google-analytics.com *.analytics.google.com https://www.compare-school-performance.service.gov.uk https://api.postcodes.io https://*.doubleclick.net https://*.clarity.ms https://c.bing.com https://*.applicationinsights.azure.com/ https://*.visualstudio.com/; child-src 'none';"
                + "frame-src 'none';"
                + "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://atlas.microsoft.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
                + "style-src 'self';"
                + "font-src 'self' data:;"
                + $"script-src 'self' 'nonce-{context.Items["ScriptNonce"]}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
                );

            await next.Invoke();
        });


        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Add health check endpoint for AKS
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
