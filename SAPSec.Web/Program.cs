using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using SAPSec.Core;
using SAPSec.Infrastructure;
using SAPSec.Web.Extensions;
using SAPSec.Web.Middleware;
using SmartBreadcrumbs.Extensions;

namespace SAPSec.Web;

// ReSharper disable once PartialTypeWithSinglePart
public partial class Program
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        builder.Services.AddRazorPages();

        builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
        {
            options.TagClasses = "govuk-breadcrumbs govuk-breadcrumbs--collapse-on-mobile";
            options.OlClasses = "govuk-breadcrumbs__list";
            options.LiTemplate = "<li class=\"govuk-breadcrumbs__list-item\"><a class=\"govuk-breadcrumbs__link\" href=\"{1}\">{0}</a></li>";
            options.ActiveLiTemplate = " ";
        });

        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        var config = configBuilder.Build();

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddDsiAuthentication(config);
        }

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/{0}.cshtml");
        });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-GB");
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB") };
            options.RequestCultureProviders.Clear();
        });

        builder.Services.AddHealthChecks();

        var dataProtectionPath = builder.Environment.IsDevelopment()
                                 ? Path.Combine(Path.GetTempPath(), "SAPSec-Test-Keys")
                                   : "/keys";

        builder.Services.AddDataProtection()
               .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
               .SetApplicationName("SAPSec");

        // Search services
        var establishmentsCsvPath = builder.Configuration["Establishments:CsvPath"];
        // if (!string.IsNullOrWhiteSpace(establishmentsCsvPath) && !Path.IsPathRooted(establishmentsCsvPath))
        // {
        //     establishmentsCsvPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, establishmentsCsvPath));
        // }

        builder.Services.AddCoreDependencies();
        builder.Services.AddInfrastructureDependencies(csvPath: establishmentsCsvPath);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 1 });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
        }

        // Security headers middleware - MUST come before static files
        app.AddMiddleware(app.Environment.IsDevelopment());

        app.UseHttpsRedirection();

        // Configure MIME types
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".css"] = "text/css";
        provider.Mappings[".js"] = "application/javascript";
        provider.Mappings[".mjs"] = "application/javascript";

        // Log wwwroot contents on startup for debugging
        var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        Console.WriteLine(Directory.Exists(wwwrootPath)
            ? $"=== wwwroot exists at: {wwwrootPath} ==="
            : $"WARNING: wwwroot directory not found at {wwwrootPath}");

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                var path = ctx.Context.Request.Path.Value;
                var contentType = ctx.Context.Response.ContentType;
                //Console.WriteLine($"Static file request: {path} -> {contentType ?? "NO CONTENT TYPE"}");

                if (string.IsNullOrEmpty(contentType))
                {
                    var ext = Path.GetExtension(path);
                    Console.WriteLine($"  WARNING: No content type for extension: {ext}");
                }
            }
        });

        app.UseRouting();

        app.UseSession();

        app.MapControllers();

        app.MapRazorPages();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Health check endpoints for AKS
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}