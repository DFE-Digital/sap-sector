using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SAPSec.Infrastructure.Data;

namespace SAPSec.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IWebHostEnvironment _environment;
        // private readonly ApplicationDbContext _dbContext;

        public HealthController(
            ILogger<HealthController> logger,
            IWebHostEnvironment environment
            // ApplicationDbContext dbContext
            )
        {
            _logger = logger;
            _environment = environment;
            // _dbContext = dbContext;
        }

        /// <summary>
        /// Health check endpoint that reports the status of the application/service
        /// Returns HTTP 200 if all checks pass, HTTP 500 if any check fails
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
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

                // Check 3: Database connection (Postgres) - COMMENTED OUT FOR NOW
                // Uncomment when database is added to the project
                // var dbConnectionCheck = await CheckDatabaseConnectionAsync();
                // healthStatus.Checks.Add(dbConnectionCheck);

                // Determine overall status
                if (healthStatus.Checks.Any(c => c.Status != "Pass"))
                {
                    healthStatus.Status = "Unhealthy";
                    _logger.LogWarning("Health check failed: {FailedChecks}",
                        string.Join(", ", healthStatus.Checks.Where(c => c.Status != "Pass").Select(c => c.Name)));
                    return StatusCode(500, healthStatus);
                }

                _logger.LogInformation("Health check passed");
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check encountered an unexpected error");
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
                var environmentName = _environment.EnvironmentName;
                var appName = _environment.ApplicationName;

                return new HealthCheckItem
                {
                    Name = "ApplicationRunning",
                    Status = "Pass",
                    Message = $"{appName} is running in {environmentName} environment"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking application status");
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
                // Check if wwwroot directory and key assets exist
                var wwwrootPath = _environment.WebRootPath;

                if (string.IsNullOrEmpty(wwwrootPath) || !Directory.Exists(wwwrootPath))
                {
                    return new HealthCheckItem
                    {
                        Name = "StaticFiles",
                        Status = "Fail",
                        Message = "wwwroot directory not found"
                    };
                }

                // Check for essential assets
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

                if (!assetsExist && !cssExists && !libExists)
                {
                    return new HealthCheckItem
                    {
                        Name = "StaticFiles",
                        Status = "Warn",
                        Message = "Some static file directories not found, but application may still function"
                    };
                }

                return new HealthCheckItem
                {
                    Name = "StaticFiles",
                    Status = "Pass",
                    Message = $"Static files accessible: {string.Join(", ", messages)}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking static files");
                return new HealthCheckItem
                {
                    Name = "StaticFiles",
                    Status = "Fail",
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // COMMENTED OUT - Uncomment when database is added to the project
        // private async Task<HealthCheckItem> CheckDatabaseConnectionAsync()
        // {
        //     try
        //     {
        //         // Try to execute a simple query to verify database connectivity
        //         var canConnect = await _dbContext.Database.CanConnectAsync();
        //
        //         if (!canConnect)
        //         {
        //             return new HealthCheckItem
        //             {
        //                 Name = "PostgresConnection",
        //                 Status = "Fail",
        //                 Message = "Cannot connect to database"
        //             };
        //         }
        //
        //         // Test a simple query
        //         await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
        //
        //         return new HealthCheckItem
        //         {
        //             Name = "PostgresConnection",
        //             Status = "Pass",
        //             Message = "Database connection working"
        //         };
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error checking database connection");
        //         return new HealthCheckItem
        //         {
        //             Name = "PostgresConnection",
        //             Status = "Fail",
        //             Message = $"Database error: {ex.Message}"
        //         };
        //     }
        // }
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<HealthCheckItem> Checks { get; set; } = new();
    }

    public class HealthCheckItem
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}