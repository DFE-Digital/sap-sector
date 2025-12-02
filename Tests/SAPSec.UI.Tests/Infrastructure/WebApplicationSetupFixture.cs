using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Xunit;

namespace SAPSec.UI.Tests.Infrastructure;
public class WebApplicationSetupFixture : IAsyncLifetime
{
    private Process? _appProcess;
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var port = GetAvailablePort();
        var webProjectPath = GetWebProjectPath();
        var testDataPath = GetTestDataPath();
        BaseUrl = $"https://localhost:{port}";

        Console.WriteLine($"📁 Starting app from: {webProjectPath}");

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                // ✅ Add --no-launch-profile to ignore launchSettings.json
                Arguments = $"run --project \"{webProjectPath}\" --no-launch-profile --urls {BaseUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        // ✅ Set environment variable
        _appProcess.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "UITesting";
        _appProcess.StartInfo.EnvironmentVariables["Establishments__CsvPath"] = testDataPath;
        _appProcess.Start();

        // Read output for debugging
        _ = Task.Run(async () =>
        {
            while (!_appProcess.StandardOutput.EndOfStream)
            {
                var line = await _appProcess.StandardOutput.ReadLineAsync();
                Console.WriteLine($"[APP] {line}");
            }
        });

        // Wait for server
        await WaitForServerAsync();

        Console.WriteLine($"✅ Test server started at: {BaseUrl}");
    }

    private string GetWebProjectPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var webProjectPath = Path.GetFullPath(
            Path.Combine(currentDir, "..", "..", "..", "..", "..", "SAPSec.Web"));

        if (!Directory.Exists(webProjectPath))
        {
            throw new DirectoryNotFoundException($"Web project not found at: {webProjectPath}");
        }

        return webProjectPath;
    }

    private async Task WaitForServerAsync()
    {
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });

        for (int i = 0; i < 60; i++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Health check passed");
                    return;
                }
            }
            catch
            {
                // Server not ready
            }
            await Task.Delay(500);
        }

        throw new TimeoutException($"Server did not start at {BaseUrl}");
    }

    public async Task DisposeAsync()
    {
        if (_appProcess != null && !_appProcess.HasExited)
        {
            _appProcess.Kill(entireProcessTree: true);
            await _appProcess.WaitForExitAsync();
            _appProcess.Dispose();
        }
    }
    private string GetTestDataPath()
    {
        // Try multiple paths to find the test data file
        var possiblePaths = new[]
        {
            // From bin/Debug/net8.0
            Path.Combine(AppContext.BaseDirectory, "TestData", "Establishments-UI-Test-Data.csv"),
            // From current directory
            Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Establishments-UI-Test-Data.csv"),
            // Up from bin
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "Establishments-UI-Test-Data.csv")),
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"✅ Found test data at: {fullPath}");
                return fullPath;
            }
        }

        throw new FileNotFoundException(
            $"Test data file not found. Searched paths:\n{string.Join("\n", possiblePaths.Select(Path.GetFullPath))}");
    }

    private static int GetAvailablePort()
    {
        // This finds an available port by briefly opening a socket
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }


}