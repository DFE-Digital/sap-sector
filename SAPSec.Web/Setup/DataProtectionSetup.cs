using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace SAPSec.Web.Setup;

/// <summary>
/// Configures ASP.NET Core Data Protection for the application.
/// Ensures data protection keys are shared across multiple pod instances in Kubernetes.
/// </summary>
public static class DataProtectionSetup
{
    private const string ApplicationName = "SAPSec";
    private const string StorageConnectionStringKey = "StorageConnectionString";
    private const string ContainerName = "keys";
    private const string BlobName = "keys.xml";
    private const string LocalKeysDirectoryName = "SAPSec-Keys";

    /// <summary>
    /// Adds data protection services with environment-specific configuration.
    /// Development: Uses local file system.
    /// Production: Uses Azure Blob Storage with encryption at rest.
    /// </summary>
    public static void AddDataProtectionServices(this WebApplicationBuilder builder)
    {
        var dataProtection = builder.Services.AddDataProtection()
            .SetApplicationName(ApplicationName);

        if (builder.Environment.IsDevelopment())
        {
            ConfigureLocalDataProtection(dataProtection);
            return;
        }

        var storageConnectionString = builder.Configuration[StorageConnectionStringKey];

        if (string.IsNullOrWhiteSpace(storageConnectionString))
        {
            ConfigureEphemeralDataProtection(dataProtection);
            return;
        }

        ConfigureAzureBlobDataProtection(dataProtection, storageConnectionString);
    }

    private static void ConfigureLocalDataProtection(IDataProtectionBuilder dataProtection)
    {
        var localPath = Path.Combine(Path.GetTempPath(), LocalKeysDirectoryName);
        Directory.CreateDirectory(localPath);

        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(localPath));

        LogDataProtectionConfiguration("Local file system", localPath);
    }

    private static void ConfigureEphemeralDataProtection(IDataProtectionBuilder dataProtection)
    {
        dataProtection.UseEphemeralDataProtectionProvider();

        LogWarning(
            "Using ephemeral data protection keys (temporary until storage is configured)",
            "Keys will be lost on pod restart - reduce to 1 replica to avoid authentication issues"
        );
    }

    private static void ConfigureAzureBlobDataProtection(
        IDataProtectionBuilder dataProtection,
        string connectionString)
    {
        try
        {
            dataProtection.PersistKeysToAzureBlobStorage(connectionString, ContainerName, BlobName);

            LogDataProtectionConfiguration(
                "Azure Blob Storage",
                $"{ContainerName}/{BlobName}",
                "Keys encrypted at rest with Azure Storage infrastructure encryption"
            );
        }
        catch (Exception ex)
        {
            LogError("Failed to configure Azure Blob Storage data protection", ex);
            throw new InvalidOperationException(
                $"Data protection setup failed. Ensure storage account is accessible and container '{ContainerName}' exists.",
                ex
            );
        }
    }

    private static void LogDataProtectionConfiguration(string provider, string location, string? additionalInfo = null)
    {
        Console.WriteLine($"Data Protection: {provider} ({location})");

        if (!string.IsNullOrWhiteSpace(additionalInfo))
        {
            Console.WriteLine($"{additionalInfo}");
        }
    }

    private static void LogWarning(params string[] messages)
    {
        foreach (var message in messages)
        {
            Console.WriteLine($"{message}");
        }
    }

    private static void LogError(string message, Exception ex)
    {
        Console.WriteLine($"{message}");
        Console.WriteLine($"Error: {ex.Message}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }
    }
}