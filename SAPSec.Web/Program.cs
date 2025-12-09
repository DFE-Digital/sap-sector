using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using SAPSec.Core;
using SAPSec.Infrastructure;
using SAPSec.Web.Authentication;
using SAPSec.Web.Extensions;
using SAPSec.Web.Middleware;
using Serilog;
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

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedHost
                                     | ForwardedHeaders.XForwardedProto
                                     | ForwardedHeaders.XForwardedFor;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

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


        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = ".SAPSec.Session";
        });

        builder.Host.UseSerilog((hostContext, services, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
                        .Enrich.WithProperty("Application", "SAPSec")
                        .WriteTo.Console();

            var logitUrl = hostContext.Configuration["LOGIT_HTTP_URL"];
            var logitApiKey = hostContext.Configuration["LOGIT_API_KEY"];

            if (!string.IsNullOrWhiteSpace(logitUrl))
            {
                try
                {
                    if (!string.IsNullOrEmpty(logitApiKey))
                    {
                        var requestUri = $"{logitUrl}?apikey={logitApiKey}";

                        Console.WriteLine($"DEBUG: Request URI = {requestUri}");

                        loggerConfig.WriteTo.Http(
                            requestUri: requestUri,
                            period: TimeSpan.FromSeconds(2),
                            queueLimitBytes: 50_000_000,
                            textFormatter: new Serilog.Formatting.Compact.RenderedCompactJsonFormatter(),
                            batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter(),
                            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug
                        );

                    }
                    else
                    {
                        Console.WriteLine("⚠️ Logit URL set but no credentials found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to configure Logit: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Logit not configured - console logging only");
            }
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = _ => false;
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
            options.Secure = CookieSecurePolicy.Always;
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

        if (builder.Environment.EnvironmentName is "IntegrationTests" or "UITests")
        {
            builder.Services.AddDataProtection()
                .UseEphemeralDataProtectionProvider()
                .SetApplicationName("SAPSec");
        }
        else
        {
            var dataProtectionPath = builder.Environment.IsDevelopment()
                                     ? Path.Combine(Path.GetTempPath(), "SAPSec-Test-Keys")
                                     : "/keys";
            try
            {
                Directory.CreateDirectory(dataProtectionPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: could not create DataProtection keys directory '{dataProtectionPath}': {ex.Message}");
            }

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
                .SetApplicationName("SAPSec");
        }

        var establishmentsCsvPath = builder.Configuration["Establishments:CsvPath"];
        builder.Services.AddCoreDependencies();
        builder.Services.AddInfrastructureDependencies(csvPath: establishmentsCsvPath);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 1 });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseForwardedHeaders();

        app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");


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

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapHealthChecks("/healthcheck");

        app.MapControllers();
        app.MapRazorPages();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}