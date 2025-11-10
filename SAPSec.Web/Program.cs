using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using SAPSec.Web.Extensions;
using SAPSec.Web.Middleware;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.HttpOverrides;
using SAPSec.Core;
using SAPSec.Infrastructure;
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
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
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
            options.Cookie.Name = ".SAPSec.Session";
        });

        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/{0}.cshtml");
        });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-GB");
            options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB") };
            options.RequestCultureProviders.Clear();
        });

        builder.Services.AddHealthChecks();

        // Search services
        var establishmentsCsvPath = builder.Configuration["Establishments:CsvPath"];
        if (!string.IsNullOrWhiteSpace(establishmentsCsvPath) && !Path.IsPathRooted(establishmentsCsvPath))
        {
            establishmentsCsvPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, establishmentsCsvPath));
        }

        builder.Services.AddCoreDependencies();
        builder.Services.AddInfrastructureDependencies(csvPath: establishmentsCsvPath);

        var app = builder.Build();
        app.UseRouting();

        app.UseSession();

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
                ForwardedHeaders = ForwardedHeaders.XForwardedHost  | ForwardedHeaders.XForwardedProto
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
        if (Directory.Exists(wwwrootPath))
        {
            Console.WriteLine($"=== wwwroot exists at: {wwwrootPath} ===");
            var files = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                .Take(20)
                .Select(f => f.Replace(wwwrootPath, ""));
            Console.WriteLine("Sample files:");
            foreach (var file in files)
            {
                Console.WriteLine($"  {file}");
            }
        }
        else
        {
            Console.WriteLine($"WARNING: wwwroot directory not found at {wwwrootPath}");
        }

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