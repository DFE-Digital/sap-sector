using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Infrastructure.Postgres;
using SAPSec.Web.Authentication;
using SAPSec.Web.Configuration;
using SAPSec.Web.Extensions;
using SAPSec.Web.Middleware;
using SAPSec.Web.Setup;
using SmartBreadcrumbs.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        builder.Services.AddRazorPages();
        builder.Services.Configure<AnalyticsSettings>(builder.Configuration.GetSection("Analytics"));
        builder.Services.PostConfigure<AnalyticsSettings>(options =>
        {
            var environmentName = builder.Configuration["ENVIRONMENT_NAME"] ?? builder.Environment.EnvironmentName;
            var analyticsEnvironment = IsProductionEnvironment(environmentName) ? "production" : "test";

            if (options.GoogleTagManagerIds?.TryGetValue(analyticsEnvironment, out var googleTagManagerId) == true)
            {
                options.GoogleTagManagerId = googleTagManagerId;
            }

            if (options.GoogleTagManagerAuths?.TryGetValue(analyticsEnvironment, out var googleTagManagerAuth) == true)
            {
                options.GoogleTagManagerAuth = googleTagManagerAuth;
            }

            if (options.GoogleTagManagerPreviews?.TryGetValue(analyticsEnvironment, out var googleTagManagerPreview) == true)
            {
                options.GoogleTagManagerPreview = googleTagManagerPreview;
            }

            if (options.ClarityIds?.TryGetValue(analyticsEnvironment, out var clarityId) == true)
            {
                options.ClarityId = clarityId;
            }
        });

        builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
        {
            options.TagClasses = "govuk-breadcrumbs govuk-breadcrumbs--collapse-on-mobile";
            options.OlClasses = "govuk-breadcrumbs__list";
            options.LiTemplate = "<li class=\"govuk-breadcrumbs__list-item\"><a class=\"govuk-breadcrumbs__link\" href=\"{1}\">{0}</a></li>";
            options.ActiveLiTemplate = " ";
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedHost
                                     | ForwardedHeaders.XForwardedProto
                                     | ForwardedHeaders.XForwardedFor;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.AddDataProtectionServices();

        if (builder.Environment.EnvironmentName is "IntegrationTests" or "UITests")
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestScheme";
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, AutoAuthenticationHandler>("TestScheme", null);
        }
        else
        {
            builder.Services.AddDsiAuthentication(builder.Configuration);
        }

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = ".SAPSec.Session";
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = _ => false;
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
            options.Secure = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
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

        var establishmentsCsvPath = builder.Configuration["Establishments:CsvPath"];

        // Add relevant dependencies for Lucene Search, implementation through SearchService.
        builder.Services.AddLuceneDependencies();

        // Service and Repo depencencies.
        builder.Services.AddPostgresqlDependencies();
        builder.Services.AddDependencies();

        // Add custom error handler for NotFoundExceptions
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

        var app = builder.Build();

        var isDevelopment = app.Environment.IsDevelopment();

        // Set up error handling
        // Note: The order of these lines is important!
        // 1. Status code pages (used by later exception handlers)
        app.UseStatusCodePagesWithReExecute("/error/{0}");
        if (isDevelopment)
        {
            // 2a. Developer exception page
            // Note: Comes first otherwise it will absorb NotFoundExceptions too which we don't want
            app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 1 });

            // 3a. Exception handler to "use" the NotFoundExceptionHandler we registered above
            // Note: empty configure block is necessary to trigger our custom exception handler
            // but without triggering the custom error page which would happen if a path was provided
            // (which would replace the developer exception page!)
            app.UseExceptionHandler(o => { });
        }
        else
        {
            // 2b. Custom error page for exceptions
            app.UseExceptionHandler("/error/500");
        }

        if (!isDevelopment)
        {
            app.UseHsts();
        }
        app.UseForwardedHeaders();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseHttpsRedirection();

        var provider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                [".css"] = "text/css",
                [".js"] = "application/javascript",
                [".mjs"] = "application/javascript"
            }
        };

        var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(wwwrootPath)) Console.WriteLine($"WARNING: wwwroot directory not found at {wwwrootPath}");

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                var path = ctx.Context.Request.Path.Value;
                var contentType = ctx.Context.Response.ContentType;

                if (string.IsNullOrEmpty(contentType))
                {
                    var ext = Path.GetExtension(path);
                    Console.WriteLine($"  WARNING: No content type for extension: {ext}");
                }
            }
        });

        app.UseRouting();

        app.UseSession();

        app.UseGovUkFrontend();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapHealthChecks("/healthcheck").AllowAnonymous();

        app.MapControllers();
        app.MapRazorPages();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    private static bool IsProductionEnvironment(string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            return false;
        }

        return environmentName.Equals("production", StringComparison.OrdinalIgnoreCase)
               || environmentName.Equals("prod", StringComparison.OrdinalIgnoreCase)
               || environmentName.Equals("pd", StringComparison.OrdinalIgnoreCase);
    }
}
