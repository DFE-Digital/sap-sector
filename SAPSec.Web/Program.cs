using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.FeatureManagement;
using SAPSec.Core;
using SAPSec.Core.Authentication;
using SAPSec.Core.Caching;
using SAPSec.Core.FeatureFlags;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Infrastructure.Postgres;
using SAPSec.Infrastructure.Redis;
using SAPSec.Web.Authentication;
using SAPSec.Web.Configuration;
using SAPSec.Web.Extensions;
using SAPSec.Web.Middleware;
using SAPSec.Web.Services;
using SAPSec.Web.Setup;
using Serilog;
using SmartBreadcrumbs.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPSec.Web;

public class Program
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var sentrySettings = SentryConfiguration.GetSettings(builder.Configuration);

        builder.WebHost.UseSentry((context, options) =>
        {
            if (!SentryConfiguration.IsEnabled(sentrySettings))
            {
                options.Dsn = "";
                return;
            }

            options.Dsn = sentrySettings.Dsn;
            options.Environment = SentryConfiguration.GetEnvironmentName(context.Configuration, context.HostingEnvironment.EnvironmentName);
            options.Debug = sentrySettings.Debug;
            options.SendDefaultPii = false;
            options.AttachStacktrace = true;
            options.MinimumBreadcrumbLevel = SentryConfiguration.GetMinimumBreadcrumbLevel(sentrySettings);
            options.MinimumEventLevel = SentryConfiguration.GetMinimumEventLevel(sentrySettings);
        });

        builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

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
        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        builder.Services.AddFeatureManagement();
        builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        builder.Services.Configure<CustomEventLocations>(builder.Configuration.GetSection("CustomEventLocations"));
        builder.Services.Configure<AnalyticsSettings>(builder.Configuration.GetSection("Analytics"));

        builder.Services.PostConfigure<AnalyticsSettings>(options =>
        {
            var environmentName = builder.Configuration["ENVIRONMENT_NAME"] ?? builder.Environment.EnvironmentName;
            var analyticsEnvironment = IsProductionEnvironment(environmentName) ? "production" : "test";

            if (options.GoogleTagManagerIds?.TryGetValue(analyticsEnvironment, out var googleTagManagerId) == true)
            {
                options.GoogleTagManagerId = googleTagManagerId;
            }

            if (options.GoogleTagManagerAdditionals?.TryGetValue(analyticsEnvironment, out var googleTagManagerAdditional) == true)
            {
                options.GoogleTagManagerAdditional = googleTagManagerAdditional;
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
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.AddDataProtectionServices();

        if (builder.Environment.EnvironmentName is "IntegrationTests" or "UITests" or "EndToEndTests" or "AccessibilityTests" or "LoadTest")
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestScheme";
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, AutoAuthenticationHandler>("TestScheme", null);

            if (builder.Environment.EnvironmentName is "LoadTest")
            {
                builder.Services.AddScoped<IUserService, MockDsiUserService>();
                builder.Services.AddScoped<IDsiClient, MockDsiApiService>();
            }
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

        builder.Services.AddDfeAnalyticsDependencies(builder.Environment);

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

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = _ => false;
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
            options.AreaViewLocationFormats.Add("/Areas/Shared/Views/{1}/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Areas/Shared/Views/{0}.cshtml");
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
        builder.Services.AddUseCases(builder.Configuration);
        builder.Services.AddServices();
        builder.Services.AddPostgresqlDependencies(builder.Configuration);
        builder.Services.AddDependencies();

        builder.Services.AddCaching(builder.Configuration, (services, config) =>
        {
            // If caching is enabled:
            services.AddRedisCache(config);
            services.AddNestedCache<RequestCache, RedisCache>();
            services.AddHostedService<StartupCacheClearer>();
        });

        // Add custom error handler for NotFoundExceptions
        builder.Services.AddProblemDetails();
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
        app.UseMiddleware<SecurityHeadersMiddleware>(app.Environment);
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
        app.UseCookiePolicy();

        app.UseSession();

        app.UseGovUkFrontend();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapHealthChecks("/healthcheck").AllowAnonymous();

        app.UseAnalytics(app.Environment);

        app.UseMiddleware<HostUrlBannerMiddleware>();

        app.MapControllers();
        app.MapRazorPages();

        app.MapControllerRoute(
          name: "Areas",
          pattern: "{area:exists}/{controller=School}/{action=Index}/{id?}");

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

    public class MockDsiUserService : IUserService
    {
        private string _currentOrganisationId = "org-123";

        private readonly User _defaultUser = new()
        {
            Sub = "test-user-123",
            Email = "test@example.com",
            GivenName = "Test",
            FamilyName = "User",
            Name = "Test User",
            Organisations = new List<Organisation>
        {
            new()
            {
                Id = "org-123",
                Name = "Test Organisation",
                LegalName = "TEST ORGANISATION LTD",
                Urn = "123456",
                Ukprn = "10012345",
                Category = new Category { Id = "001", Name = "Establishment" },
                Type = new Core.Authentication.Type { Id = "34", Name = "Academy Converter" },
                Status = new Status { Id = 1, Name = "Open", TagColor = "green" },
                Address = "123 Test Street, Test Town, TS1 1TT",
                PhaseOfEducation = new PhaseOfEducation { Id = 2, Name = "Primary" }
            }
        }
        };

        public Task<User?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(_defaultUser);
        }

        public Task<Organisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult<Organisation?>(null);
            }

            var org = _defaultUser.Organisations.FirstOrDefault(o => o.Id == _currentOrganisationId)
                      ?? _defaultUser.Organisations.FirstOrDefault();

            return Task.FromResult(org);
        }

        public Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult(false);
            }

            var org = _defaultUser.Organisations.FirstOrDefault(o => o.Id == organisationId);
            if (org == null)
            {
                return Task.FromResult(false);
            }

            _currentOrganisationId = organisationId;
            return Task.FromResult(true);
        }

        public string? GetUserId(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? principal.FindFirst("sub")?.Value
                   ?? _defaultUser.Sub;
        }

        public string? GetUserEmail(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return principal.FindFirst(ClaimTypes.Email)?.Value
                   ?? principal.FindFirst("email")?.Value
                   ?? _defaultUser.Email;
        }

        public bool IsAuthenticated(ClaimsPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated == true;
        }

        public bool HasRole(ClaimsPrincipal principal, string role)
        {
            return principal?.IsInRole(role) == true;
        }
    }

    public class MockDsiApiService : IDsiClient
    {
        private readonly Dictionary<string, UserInfo> _users;
        private readonly Dictionary<string, Organisation> _organisations;

        public MockDsiApiService()
        {
            // Setup default test data
            _organisations = new Dictionary<string, Organisation>
            {
                ["org-123"] = new Organisation
                {
                    Id = "org-123",
                    Name = "Test Organisation",
                    LegalName = "TEST ORGANISATION LTD",
                    Urn = "123456",
                    Ukprn = "10012345",
                    EstablishmentNumber = "1234",
                    Category = new Category { Id = "001", Name = "Establishment" },
                    Type = new Core.Authentication.Type { Id = "34", Name = "Academy Converter" },
                    Status = new Status { Id = 1, Name = "Open", TagColor = "green" },
                    Address = "123 Test Street, Test Town, TS1 1TT",
                    Telephone = "01onal234567890",
                    Region = new Region { Id = "D", Name = "Yorkshire and the Humber" },
                    LocalAuthority = new LocalAuthority
                    {
                        Id = "LA-123",
                        Name = "Test Council",
                        Code = "123"
                    },
                    PhaseOfEducation = new PhaseOfEducation { Id = 2, Name = "Primary" },
                    StatutoryLowAge = 3,
                    StatutoryHighAge = 11
                },
                ["org-456"] = new Organisation
                {
                    Id = "org-456",
                    Name = "Secondary Test School",
                    LegalName = "SECONDARY TEST SCHOOL",
                    Urn = "654321",
                    Ukprn = "10054321",
                    Category = new Category { Id = "001", Name = "Establishment" },
                    Type = new Core.Authentication.Type { Id = "34", Name = "Academy Converter" },
                    Status = new Status { Id = 1, Name = "Open", TagColor = "green" },
                    PhaseOfEducation = new PhaseOfEducation { Id = 4, Name = "Secondary" },
                    StatutoryLowAge = 11,
                    StatutoryHighAge = 16
                }
            };

            _users = new Dictionary<string, UserInfo>
            {
                ["test-user-123"] = new UserInfo
                {
                    Email = "test@example.com",
                    GivenName = "Test",
                    FamilyName = "User",
                    Organisations = new List<Organisation> { _organisations["org-123"] }
                },
                ["test-user-456"] = new UserInfo
                {
                    Email = "another@example.com",
                    GivenName = "Another",
                    FamilyName = "User",
                    Organisations = new List<Organisation>
                {
                    _organisations["org-123"],
                    _organisations["org-456"]
                }
                }
            };
        }

        public Task<UserInfo?> GetUserAsync(string userId)
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<UserInfo?> GetUserByEmailAsync(string email)
        {
            var user = _users.Values.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<Organisation?> GetOrganisationAsync(string organisationId)
        {
            _organisations.TryGetValue(organisationId, out var org);
            return Task.FromResult(org);
        }

        public Task<string> GenerateBearerToken()
        {
            // Return a fake token for testing
            return Task.FromResult("test-bearer-token-12345");
        }
    }
}
