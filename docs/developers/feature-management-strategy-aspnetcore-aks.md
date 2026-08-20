# Feature Management Strategy for ASP.NET Core on AKS

## Purpose

This document explains how we should introduce feature management in our ASP.NET Core/.NET application hosted on AKS when we do **not** currently have a dedicated feature flag platform such as Azure App Configuration or LaunchDarkly.

The goal is to give the team a clear decision model for:

- what belongs in `appsettings`
- what belongs in Terraform-managed environment configuration
- how to use `Microsoft.FeatureManagement` in ASP.NET Core
- how to avoid mixing feature flags, application settings, and infrastructure configuration incorrectly

---

## Executive Summary

In our current setup, the recommended approach is:

| Configuration type | Recommended location |
|---|---|
| Application defaults | `appsettings.json` |
| Environment-specific values | Terraform variables / Helm values / Kubernetes environment variables |
| Feature flag values that vary by environment | Terraform-managed environment variables |
| Local developer defaults | `appsettings.Development.json` or user secrets |
| Runtime business-controlled feature toggles | Not ideal without Azure App Configuration/LaunchDarkly; requires redeploy/restart in our current model |

In simple terms:

> Use `appsettings` for application-owned defaults and structure.  
> Use Terraform variables for environment-specific values, including feature flag overrides per environment.  
> Use `Microsoft.FeatureManagement` to consume those values cleanly in code.

---

## What Is a Feature Flag?

A feature flag is a named switch that controls whether a feature or code path is enabled.

Example:

```csharp
if (await featureManager.IsEnabledAsync("NewPaymentsFlow"))
{
    // Use new payment flow
}
else
{
    // Use existing payment flow
}
```

Feature flags help us:

- release code without immediately exposing functionality
- enable features gradually by environment
- reduce deployment risk
- support dark launches
- switch off risky functionality quickly, subject to how the configuration is delivered

---

## Important Limitation in Our Current Setup

Because we do not currently have Azure App Configuration, LaunchDarkly, or another dynamic feature flag provider, our feature flags are effectively **deployment-time configuration**.

That means changing a flag will usually require one of the following:

- updating Terraform variables and redeploying the AKS workload
- updating Kubernetes config/environment variables and restarting pods
- changing `appsettings` and rebuilding/redeploying the application image, if the value is baked into the image

So our current approach is good for:

- environment-based enablement
- controlled releases
- non-urgent toggles
- feature rollout by deployment pipeline

It is not ideal for:

- product-owner self-service toggles
- instant production switch-off without deployment
- user percentage rollout
- per-customer targeting
- A/B testing

For those scenarios, we should later consider Azure App Configuration or a dedicated feature flag service.

---

## AppSettings vs Terraform Variables

### AppSettings

`appsettings.json` belongs to the application.

Use it for values that are:

- application defaults
- strongly related to code behaviour
- safe to define in source control
- common across environments unless overridden
- required for local development

Examples:

```json
{
  "FeatureManagement": {
    "NewPaymentsFlow": false,
    "UseNewSearchApi": false
  },
  "Payments": {
    "RetryCount": 3,
    "TimeoutSeconds": 30
  }
}
```

Good candidates for `appsettings`:

- default feature flag values
- non-secret application defaults
- retry counts
- timeout defaults
- logging defaults
- feature flag names/structure
- local development behaviour

Avoid putting these only in `appsettings` when they differ between environments and are managed through deployment:

- production-only feature enablement
- test/staging toggles
- values that release managers or DevOps need to control
- values that must not require an application image rebuild

---

### Terraform Variables

Terraform variables belong to infrastructure and environment deployment configuration.

Use them for values that are:

- different per environment
- controlled through release/deployment process
- part of AKS workload configuration
- expected to be injected as Kubernetes environment variables or Helm values
- not hardcoded into the application image

Examples:

```hcl
variable "feature_new_payments_flow_enabled" {
  type    = bool
  default = false
}

variable "feature_use_new_search_api_enabled" {
  type    = bool
  default = false
}
```

Example environment values:

```hcl
# dev.tfvars
feature_new_payments_flow_enabled = true
feature_use_new_search_api_enabled = true

# test.tfvars
feature_new_payments_flow_enabled = true
feature_use_new_search_api_enabled = false

# prod.tfvars
feature_new_payments_flow_enabled = false
feature_use_new_search_api_enabled = false
```

Good candidates for Terraform variables:

- environment-specific feature flag values
- API base URLs
- external service endpoints
- replica counts
- resource limits
- ingress settings
- environment names
- configuration that changes by environment

Do not use Terraform variables for:

- feature flag names used by code
- complex business rules
- values developers need for local development only
- frequently changing operational switches that need instant runtime changes
- anything that business users expect to change without a deployment

---

## Decision Matrix

| Question | Use `appsettings` | Use Terraform variable |
|---|---:|---:|
| Is this an application default? | Yes | No |
| Does the value differ by environment? | Maybe as default | Yes |
| Should DevOps/release pipeline control it? | No | Yes |
| Should it be available locally without AKS? | Yes | No |
| Does changing it require infrastructure/workload redeployment? | No, ideally | Yes |
| Is it a feature flag default? | Yes | No |
| Is it the actual environment-specific feature flag value? | No | Yes |
| Is it secret? | No | No — use Key Vault/Kubernetes Secret instead |
| Is it a business toggle needing instant runtime change? | Not ideal | Not ideal |

---

## Recommended Pattern

We should use a layered configuration approach.

### 1. Define safe defaults in `appsettings.json`

```json
{
  "FeatureManagement": {
    "NewPaymentsFlow": false,
    "UseNewSearchApi": false,
    "EnableExperimentalDashboard": false
  }
}
```

These defaults make the application runnable locally and safe by default.

### 2. Override per environment using Terraform/Kubernetes environment variables

ASP.NET Core configuration supports overriding nested configuration using double underscores.

For example:

```bash
FeatureManagement__NewPaymentsFlow=true
FeatureManagement__UseNewSearchApi=false
```

These environment variables override the values in `appsettings.json`.

### 3. Manage environment-specific values in Terraform

Example Terraform variables:

```hcl
variable "feature_new_payments_flow_enabled" {
  type        = bool
  description = "Controls whether the new payments flow is enabled."
  default     = false
}

variable "feature_use_new_search_api_enabled" {
  type        = bool
  description = "Controls whether the new search API is enabled."
  default     = false
}
```

Example Kubernetes deployment environment variables:

```hcl
env {
  name  = "FeatureManagement__NewPaymentsFlow"
  value = tostring(var.feature_new_payments_flow_enabled)
}

env {
  name  = "FeatureManagement__UseNewSearchApi"
  value = tostring(var.feature_use_new_search_api_enabled)
}
```

If we use Helm, the same idea applies through Helm values:

```yaml
env:
  - name: FeatureManagement__NewPaymentsFlow
    value: "{{ .Values.features.newPaymentsFlow }}"
  - name: FeatureManagement__UseNewSearchApi
    value: "{{ .Values.features.useNewSearchApi }}"
```

---

## Why Not Put All Feature Flags Only in Terraform?

Terraform is suitable for **environment-specific feature flag values**, but it should not become the only place where feature management exists.

Reasons:

1. Developers need local defaults.
2. The application should document the expected flags.
3. Code should remain understandable without reading Terraform.
4. New environments should be safe by default.
5. Terraform is not a dynamic feature flag platform.

Recommended compromise:

> Define feature flag defaults in `appsettings.json`.  
> Override values per environment using Terraform-managed environment variables.

---

## Why Not Put All Feature Flags Only in AppSettings?

Using only `appsettings` can be acceptable for small applications, but it becomes less suitable in AKS when values differ by environment.

Problems:

1. Environment-specific values may get committed into source control.
2. Production changes may require application image rebuilds.
3. Release teams have less control.
4. It becomes harder to promote the same container image across environments.
5. Configuration and code become too tightly coupled.

In AKS, we should aim to build the image once and promote it through environments with external configuration.

---

## Recommended Rules

### Rule 1: Application-owned defaults go in `appsettings.json`

Example:

```json
{
  "FeatureManagement": {
    "NewPaymentsFlow": false
  }
}
```

### Rule 2: Environment-specific overrides go through Terraform

Example:

```bash
FeatureManagement__NewPaymentsFlow=true
```

### Rule 3: Keep production defaults safe

New features should usually default to `false`.

### Rule 4: Feature flags should be temporary unless they are operational toggles

Once a feature is fully released and stable, remove the flag and clean up the old code path.

### Rule 5: Do not use feature flags for secrets

Feature flags are not secrets. Use Key Vault or Kubernetes Secrets for sensitive values.

### Rule 6: Avoid long-lived flag debt

Every feature flag should have:

- an owner
- a purpose
- a default value
- environments where it is enabled
- a planned removal date or review date

---

## Microsoft.FeatureManagement in ASP.NET Core

`Microsoft.FeatureManagement` is Microsoft’s feature flag library for .NET applications. It is built on top of the standard .NET configuration system, so it can read feature flags from providers such as:

- `appsettings.json`
- environment variables
- Kubernetes configuration
- Azure App Configuration, if added later
- any other `IConfiguration` provider

Microsoft’s documentation describes the library as a way to expose application functionality based on feature flags, with support for JSON configuration, dependency injection, request-consistent flag evaluation, MVC integration, filters, routing, and more.

---

## NuGet Package

For ASP.NET Core applications, install:

```bash
dotnet add package Microsoft.FeatureManagement.AspNetCore
```

---

## Basic Configuration

### appsettings.json

```json
{
  "FeatureManagement": {
    "NewPaymentsFlow": false,
    "UseNewSearchApi": true
  }
}
```

This is the simple configuration format supported by `Microsoft.FeatureManagement`.

---

## Program.cs Setup

```csharp
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddFeatureManagement();

var app = builder.Build();

app.MapControllers();

app.Run();
```

---

## Using Feature Flags in Application Code

Inject `IFeatureManager`.

```csharp
using Microsoft.FeatureManagement;

public class PaymentService
{
    private readonly IFeatureManager _featureManager;

    public PaymentService(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task ProcessPaymentAsync()
    {
        if (await _featureManager.IsEnabledAsync("NewPaymentsFlow"))
        {
            await ProcessUsingNewFlowAsync();
            return;
        }

        await ProcessUsingExistingFlowAsync();
    }

    private Task ProcessUsingNewFlowAsync()
    {
        // New implementation
        return Task.CompletedTask;
    }

    private Task ProcessUsingExistingFlowAsync()
    {
        // Existing implementation
        return Task.CompletedTask;
    }
}
```

---

## Strongly-Typed Feature Flag Names

Avoid using string literals throughout the codebase.

Recommended:

```csharp
public static class FeatureFlags
{
    public const string NewPaymentsFlow = "NewPaymentsFlow";
    public const string UseNewSearchApi = "UseNewSearchApi";
    public const string EnableExperimentalDashboard = "EnableExperimentalDashboard";
}
```

Usage:

```csharp
if (await _featureManager.IsEnabledAsync(FeatureFlags.NewPaymentsFlow))
{
    // New flow
}
```

---

## Using Feature Gates on Controllers or Actions

The ASP.NET Core package supports attributes such as `FeatureGate`.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost("new")]
    [FeatureGate(FeatureFlags.NewPaymentsFlow)]
    public IActionResult ProcessUsingNewFlow()
    {
        return Ok("New payments flow is enabled.");
    }
}
```

If the feature is disabled, access to the action is blocked.

This is useful when an entire endpoint should only exist when a feature is enabled.

---

## Using Feature Flags in Minimal APIs

```csharp
app.MapGet("/api/dashboard", async (IFeatureManager featureManager) =>
{
    if (!await featureManager.IsEnabledAsync(FeatureFlags.EnableExperimentalDashboard))
    {
        return Results.NotFound();
    }

    return Results.Ok("Experimental dashboard enabled.");
});
```

---

## Using Feature Flags in Razor Views

If the application uses MVC/Razor views, feature flags can be used to conditionally render UI.

```cshtml
@inject Microsoft.FeatureManagement.IFeatureManager FeatureManager

@if (await FeatureManager.IsEnabledAsync("EnableExperimentalDashboard"))
{
    <a href="/dashboard">New Dashboard</a>
}
```

---

## Handling Request Consistency

For web requests, prefer `IFeatureManagerSnapshot` where request-level consistency is important.

```csharp
using Microsoft.FeatureManagement;

public class DashboardService
{
    private readonly IFeatureManagerSnapshot _featureManager;

    public DashboardService(IFeatureManagerSnapshot featureManager)
    {
        _featureManager = featureManager;
    }
}
```

`IFeatureManagerSnapshot` keeps feature flag evaluation consistent during a request.

This avoids a scenario where a flag is evaluated as `true` in one part of the request and `false` in another if configuration refresh happens during the request.

In our current Terraform/environment-variable model, configuration is not dynamically changing during a request, but `IFeatureManagerSnapshot` is still a good habit in request-scoped services.

---

## Naming Convention

Use clear, positive, business-readable names.

Good:

```text
NewPaymentsFlow
UseNewSearchApi
EnableExperimentalDashboard
```

Avoid:

```text
DisableOldThing
TempFlag
HariTestFlag
Flag1
```

Recommended naming pattern:

```text
<Verb><FeatureName>
```

Examples:

```text
EnableNewDashboard
UseNewPaymentProvider
ShowBetaSearchResults
```

---

## Example End-to-End Setup

### appsettings.json

```json
{
  "FeatureManagement": {
    "NewPaymentsFlow": false,
    "UseNewSearchApi": false
  }
}
```

### Terraform variable

```hcl
variable "feature_new_payments_flow_enabled" {
  type    = bool
  default = false
}
```

### AKS environment variable

```hcl
env {
  name  = "FeatureManagement__NewPaymentsFlow"
  value = tostring(var.feature_new_payments_flow_enabled)
}
```

### C# feature flag constant

```csharp
public static class FeatureFlags
{
    public const string NewPaymentsFlow = "NewPaymentsFlow";
}
```

### C# usage

```csharp
if (await featureManager.IsEnabledAsync(FeatureFlags.NewPaymentsFlow))
{
    // New behaviour
}
else
{
    // Existing behaviour
}
```

---

## Unit Testing Feature-Flagged Code

Where possible, avoid testing `Microsoft.FeatureManagement` directly. Instead, isolate the behaviour and test both branches.

Example using a wrapper:

```csharp
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName);
}
```

Implementation:

```csharp
using Microsoft.FeatureManagement;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager;

    public FeatureFlagService(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public Task<bool> IsEnabledAsync(string featureName)
    {
        return _featureManager.IsEnabledAsync(featureName);
    }
}
```

This makes application services easier to unit test.

---

## Recommended Team Decision

For our AKS-hosted ASP.NET Core application:

1. Use `Microsoft.FeatureManagement.AspNetCore` in the application.
2. Keep safe feature defaults in `appsettings.json`.
3. Use Terraform variables to control feature values per environment.
4. Inject those values into AKS as environment variables.
5. Keep flag names strongly typed in code.
6. Maintain a feature flag register.
7. Remove temporary flags after rollout.
8. Revisit Azure App Configuration or LaunchDarkly if we need dynamic runtime toggling, percentage rollout, targeting, audit history, or business self-service.

---

## References

- Microsoft Learn: .NET Feature Management  
  https://learn.microsoft.com/en-us/azure/azure-app-configuration/feature-management-dotnet-reference

- Microsoft Learn: Quickstart for adding feature flags to ASP.NET Core apps  
  https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-feature-flag-aspnet-core

- Microsoft GitHub: FeatureManagement-Dotnet  
  https://github.com/microsoft/FeatureManagement-Dotnet
