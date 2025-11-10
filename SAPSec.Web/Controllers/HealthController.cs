using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Domain;

namespace SAPSec.Web.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class HealthController(
        ILogger<HealthController> logger,
        IWebHostEnvironment environment) : ControllerBase
    {
        /// <summary>
        /// Health check endpoint that reports the status of the application/service
        /// Returns HTTP 200 if all checks pass, HTTP 500 if any check fails
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            var healthStatus = new HealthCheckResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Checks = new List<HealthCheckItem>()
            };

            try
            {
                // Check 1: Application is running
                var appRunningCheck = CheckApplicationRunning();
                healthStatus.Checks.Add(appRunningCheck);

                // Check 2: Static files exist (wwwroot)
                var staticFilesCheck = CheckStaticFilesExist();
                healthStatus.Checks.Add(staticFilesCheck);

                // Determine overall status
                if (healthStatus.Checks.Any(c => c.Status == "Fail"))
                {
                    healthStatus.Status = "Unhealthy";
                    logger.LogWarning("Health check failed: {FailedChecks}",
                        string.Join(", ", healthStatus.Checks.Where(c => c.Status != "Pass").Select(c => c.Name)));
                    return StatusCode(500, healthStatus);
                }

                logger.LogInformation("Health check passed");
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check encountered an unexpected error: {Message}", ex.Message);
                healthStatus.Status = "Unhealthy";
                healthStatus.Checks.Add(new HealthCheckItem
                {
                    Name = "UnexpectedError",
                    Status = "Fail",
                    Message = ex.Message
                });
                return StatusCode(500, healthStatus);
            }
        }

        private HealthCheckItem CheckApplicationRunning()
        {
            try
            {
                // Basic check that the application is responding
                var environmentName = environment.EnvironmentName;
                var appName = environment.ApplicationName;

                return new HealthCheckItem
                {
                    Name = "ApplicationRunning",
                    Status = "Pass",
                    Message = $"{appName} is running in {environmentName} environment"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking application status");
                return new HealthCheckItem
                {
                    Name = "ApplicationRunning",
                    Status = "Fail",
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private HealthCheckItem CheckStaticFilesExist()
        {
            try
            {
                // Check if wwwroot directory exists
                var wwwrootPath = environment.WebRootPath;

                // If WebRootPath is null, that's not necessarily a failure in test environment
                if (string.IsNullOrEmpty(wwwrootPath))
                {
                    return new HealthCheckItem
                    {
                        Name = "StaticFiles",
                        Status = "Warn",
                        Message = "WebRootPath is not configured (normal in test environment)"
                    };
                }

                if (!Directory.Exists(wwwrootPath))
                {
                    return new HealthCheckItem
                    {
                        Name = "StaticFiles",
                        Status = "Warn",
                        Message = "wwwroot directory not found (may be normal in test environment)"
                    };
                }

                // Check for essential assets - but don't fail if they don't exist
                var assetsPath = Path.Combine(wwwrootPath, "assets");
                var cssPath = Path.Combine(wwwrootPath, "css");
                var libPath = Path.Combine(wwwrootPath, "lib");

                var assetsExist = Directory.Exists(assetsPath);
                var cssExists = Directory.Exists(cssPath) || System.IO.File.Exists(Path.Combine(wwwrootPath, "css", "site.css"));
                var libExists = Directory.Exists(libPath);

                var messages = new List<string>();
                if (assetsExist) messages.Add("assets OK");
                if (cssExists) messages.Add("CSS OK");
                if (libExists) messages.Add("libraries OK");

                // Even if some directories are missing, mark as Pass or Warn
                var status = (assetsExist || cssExists || libExists) ? "Pass" : "Warn";

                return new HealthCheckItem
                {
                    Name = "StaticFiles",
                    Status = status,
                    Message = messages.Any()
                        ? $"Static files accessible: {string.Join(", ", messages)}"
                        : "No static file directories found but application is running"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking static files");
                return new HealthCheckItem
                {
                    Name = "StaticFiles",
                    Status = "Warn",
                    Message = $"Could not verify static files: {ex.Message}"
                };
            }
        }
    }
}