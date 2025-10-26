using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SAPSec.Web.Middleware;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Azure.Identity;
using SAPSec.Core.Services;

namespace SAPSec.Web;

public partial class Program
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var keyVaultName = builder.Configuration["KeyVaultName"];
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
        }

        builder.Services.AddGovUkFrontend(options =>
        {
            options.Rebrand = true;
        });

        // Single, consolidated controllers registration
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        // Add Razor pages separately if needed
        builder.Services.AddRazorPages();

        // View configuration
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

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None; // Required for DSI redirects
        });

        builder.Services.AddHttpContextAccessor();

        var dsiClientId = builder.Configuration["DsiClientId"];
        var dsiClientSecret = builder.Configuration["DsiClientSecret"];
        var dsiAuthority = builder.Configuration["DsiAuthority"];

        var dsiEnabled = !string.IsNullOrEmpty(dsiClientId) &&
                        !string.IsNullOrEmpty(dsiClientSecret) &&
                        !string.IsNullOrEmpty(dsiAuthority);
        if (dsiEnabled)
        {
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = ".SAPSec.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/access-denied";
                })
                .AddOpenIdConnect(options =>
                {
                    // DSI Configuration from Key Vault
                    options.Authority = dsiAuthority;
                    options.ClientId = dsiClientId;
                    options.ClientSecret = dsiClientSecret;

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;

                    options.CallbackPath = "/signin-oidc";
                    options.SignedOutCallbackPath = "/signout-callback-oidc";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.UseTokenLifetime = false;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");  
                    options.Scope.Add("profile");      
                    options.Scope.Add("organisation");

                    options.MapInboundClaims = false;

                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "role";

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();

                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            var orgClaim = context.Principal?.FindFirst("organisation")?.Value;

                            logger.LogInformation(
                                "DSI authentication successful for user: {UserId}",
                                userId);

                            if (!string.IsNullOrEmpty(orgClaim) && orgClaim != "{}")
                            {
                                context.HttpContext.Session.SetString("Organisation", orgClaim);
                                logger.LogInformation("Organization stored in session");
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();

                            logger.LogError(
                                context.Exception,
                                "DSI authentication failed");

                            context.HandleResponse();
                            context.Response.Redirect("/error?message=Authentication failed");

                            return Task.CompletedTask;
                        },

                        OnRemoteFailure = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();

                            logger.LogError(
                                context.Failure,
                                "DSI remote authentication failure");

                            context.HandleResponse();
                            context.Response.Redirect("/error");

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });

            Console.WriteLine("✅ DSI Authentication configured");
        }
        else
        {
            Console.WriteLine("⚠️ DSI Authentication not configured - running without auth");
        }

        builder.Services.AddScoped<IOrganisationService, OrganisationService>();
        builder.Services.AddHealthChecks();
        builder.Services.AddDataProtection()
               .PersistKeysToFileSystem(new DirectoryInfo(@"/keys"))
               .SetApplicationName("SAPSec");

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

        app.UseSecurityHeaders();

        app.UseHttpsRedirection();

        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".css"] = "text/css";
        provider.Mappings[".js"] = "application/javascript";
        provider.Mappings[".mjs"] = "application/javascript";

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
                Console.WriteLine($"Static file request: {path} -> {contentType ?? "NO CONTENT TYPE"}");

                if (string.IsNullOrEmpty(contentType))
                {
                    var ext = Path.GetExtension(path);
                    Console.WriteLine($"  WARNING: No content type for extension: {ext}");
                }
            }
        });

        app.UseRouting();

        app.UseSession();
        if (dsiEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseGovUkFrontend();

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