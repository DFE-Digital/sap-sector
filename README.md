<a id="readme-top"></a>

# SAP Sector — Get School Improvement Insights (Sector Facing)

[![Build and Deploy](https://github.com/DFE-Digital/sap-sector/actions/workflows/build-and-deploy.yml/badge.svg)](https://github.com/DFE-Digital/sap-sector/actions/workflows/build-and-deploy.yml)
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/DFE-Digital/sap-sector/">
    <img src="docs/logo.png" alt="Logo" width="80" height="80">
  </a>

<h3 align="center">Get School Improvement Insights</h3>

  <p align="center">
    Sector facing output of the SAP (School Account Profile) project — an authenticated ASP.NET Core MVC service that lets schools, trusts and local authorities search for schools, view school detail, and compare against similar schools.
    <br />
    <br />
    <a href="https://get-school-improvement-insights.education.gov.uk">View Service</a>
    ·
    <a href="https://github.com/DFE-Digital/sap-sector/issues/new?labels=bug">Report Bug</a>
    ·
    <a href="https://github.com/DFE-Digital/sap-sector/issues/new?labels=enhancement">Request Feature</a>
    ·
    <a href="docs/">Documentation</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
## Table of Contents

1. [About The Project](#about-the-project)
   - [Environments](#environments)
   - [Technology](#technology)
   - [Solution Structure](#solution-structure)
2. [Getting Started](#getting-started)
   - [Prerequisites](#prerequisites)
   - [Installation](#installation)
   - [User Secrets](#user-secrets)
   - [Database Setup](#database-setup)
3. [Running Locally](#running-locally)
4. [Running with Docker](#running-with-docker)
5. [Key Features](#key-features)
6. [Configuration](#configuration)
7. [Routes and Endpoints](#routes-and-endpoints)
8. [Health Checks](#health-checks)
9. [Testing](#testing)
10. [Data Pipeline](#data-pipeline)
11. [Deployment](#deployment)
12. [Infrastructure](#infrastructure)
13. [Documentation](#documentation)
14. [Contributing](#contributing)
15. [License](#license)
16. [Contact](#contact)

<!-- ABOUT THE PROJECT -->
## About The Project

This repository contains the **sector facing** service for the School Improvement Programme (SIP) / School Account Profile (SAP) project. It is a .NET 8 solution made up of an ASP.NET Core MVC web application, supporting class libraries for domain logic and infrastructure, and a data platform used to ingest and curate the underlying education datasets.

The service is **authenticated by default**. Users sign in through **DfE Sign-in (DSI)**, and the application applies a fallback authorization policy so routes are protected unless explicitly opened. It supports sector users — schools, trusts and local authority users — across three main journeys:

1. **Find a school** — search by name, URN or location
2. **View school details** — performance, attendance and establishment data
3. **Compare with similar schools** — side-by-side comparison against a derived similar-schools group

The application uses the GOV.UK Design System and DfE Frontend for a consistent, accessible user experience.

### Environments

| Environment | Public URL | Internal URL | Status |
| --- | --- | --- | --- |
| Production | https://get-school-improvement-insights.education.gov.uk | `get-school-improvement-insights-production.teacherservices.cloud` | ![Production](https://img.shields.io/github/deployments/DFE-Digital/sap-sector/production) |
| Test | https://test.get-school-improvement-insights.education.gov.uk | `get-school-improvement-insights-test.test.teacherservices.cloud` | ![Test](https://img.shields.io/github/deployments/DFE-Digital/sap-sector/test) |
| Review | Per-PR, published to the PR's **Environments** section | `*.test.teacherservices.cloud` | ![Review](https://img.shields.io/github/deployments/DFE-Digital/sap-sector/review) |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Technology

<a href="https://docs.microsoft.com/en-us/dotnet/csharp/"><img src="https://img.shields.io/badge/language-C%23-%23178600" title="Go To C# Documentation"></a>
<a href="https://dotnet.microsoft.com/download"><img src="https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=target&query=%2F%2FTargetFramework%5B1%5D&url=https://raw.githubusercontent.com/DFE-Digital/sap-sector/main/SAPSec.Web/SAPSec.Web.csproj&logo=.net" title="Go To .NET Download"></a>
<a href="https://github.com/DFE-Digital/sap-sector"><img src="https://img.shields.io/badge/github-repo-%2324292e?logo=github" title="Go To Github Repo"></a>
<a href="https://www.postgresql.org/"><img src="https://img.shields.io/badge/database-PostgreSQL-%23336791?logo=postgresql&logoColor=white" title="PostgreSQL"></a>
<a href="https://www.terraform.io/"><img src="https://img.shields.io/badge/IaC-Terraform-%237B42BC?logo=terraform&logoColor=white" title="Terraform"></a>

| Concern | Technology |
| --- | --- |
| Runtime | .NET 8 / ASP.NET Core MVC |
| Frontend | GOV.UK Frontend 5.x, DfE Frontend 2.x, `GovUk.Frontend.AspNetCore`, Sass + Gulp |
| Charts & maps | Chart.js, Leaflet + MarkerCluster, accessible-autocomplete |
| Data access | PostgreSQL via Dapper + Npgsql |
| Search | Lucene.NET 4.8 |
| Auth | DfE Sign-in (OpenID Connect) |
| Feature flags | `Microsoft.FeatureManagement` |
| Logging & monitoring | Serilog (compact JSON), Sentry, Logit, DfE Analytics (BigQuery) |
| Hosting | Docker on Azure Kubernetes Service (Teacher Services Cloud) |
| IaC | Terraform 1.14.x |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Solution Structure

```
sap-sector/
├── .github/
│   ├── PULL_REQUEST_TEMPLATE.MD
│   └── workflows/                    # CI/CD, data pipeline and DB operations
├── SAPSec.Web/                       # ASP.NET Core MVC presentation layer
│   ├── Areas/
│   │   ├── Primary/                  # Primary school journeys (KS2, attendance, similar schools)
│   │   └── Secondary/                # Secondary school journeys (KS4, destinations, attendance)
│   ├── Authentication/               # DSI OIDC handlers + test auto-auth handler
│   ├── Authorization/                # Policies and requirements
│   ├── Configuration/                # Sentry, analytics and options binding
│   ├── Controllers/                  # Auth, Home, SchoolSearch, Health, Error, StaticContent, User…
│   ├── Middleware/                   # SecurityHeadersMiddleware, NotFoundExceptionHandler
│   ├── AssetSrc/                     # Source Sass, JS and images (compiled by Gulp)
│   ├── Views/ ViewModels/ ViewComponents/ TagHelpers/
│   ├── wwwroot/                      # Compiled static assets (generated, not committed)
│   ├── gulpfile.cjs · package.json   # Frontend build
│   └── Program.cs                    # Application entry point
├── SAPSec.Core/                      # Domain models, use cases, rules, mappers, interfaces
├── SAPSec.Infrastructure/            # Postgres repositories, Lucene search, JSON loaders
├── Data/
│   ├── SAPSec.Data/                  # DTOs and repositories shared with the data tooling
│   ├── SAPSec.Data.Common/           # Data-map primitives
│   ├── SAPSec.DtoGenerator/          # Generates DTOs from the data map
│   └── SAPSec.PrimaryJsonFileGenerator/
├── SAPData/                          # Data ingestion + SQL generation console app (see SAPData/README.md)
│   ├── DataMap/                      # SourceFiles (git-ignored) and CleanedFiles
│   ├── Hashes/ · Scripts/ · Sql/     # Change detection, helpers and generated SQL
├── Tests/
│   ├── SAPSec.Core.Tests/            # Unit tests — features, mappers, rules, services
│   ├── SAPSec.Infrastructure.Tests/  # Repository and Lucene tests
│   ├── SAPSec.Test.Common/           # Shared builders, fixtures, Playwright helpers
│   ├── SAPSec.Test.Integration/      # In-process HTTP/integration tests
│   ├── SAPSec.Test.EndToEnd/         # Playwright end-to-end journeys
│   ├── SAPSec.Test.Accessibility/    # Automated accessibility checks
│   ├── SAPSec.Web.Tests/             # (Deprecated — superseded by the above)
│   └── SAPSec.UI.Tests/              # (Deprecated — superseded by the above)
├── terraform/
│   ├── application/                  # AKS app, Postgres, storage, DfE Analytics
│   └── domains/                      # DNS zone and Front Door configuration
├── maintenance_page/                 # Standalone nginx maintenance site + manifests
├── global_config/                    # Per-environment Azure/Terraform variables
├── docs/                             # Architecture, ADRs, developer, testing and ops docs
├── Dockerfile · Makefile · SAPSec.sln
└── build · build.bat · watch · watch.bat
```

> Code is organised by technical concern rather than feature folders. See [`docs/developers/project-structure.md`](docs/developers/project-structure.md) before adding new code.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22.x](https://nodejs.org/) (Node 20+ is the minimum supported)
- [PostgreSQL](https://www.postgresql.org/download/) — local install or container, with `psql` available
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerised development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/) or [VS Code](https://code.visualstudio.com/)
- [Terraform 1.14.5](https://developer.hashicorp.com/terraform/downloads) and the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (only for infrastructure work)

Verify your toolchain:

```bash
dotnet --version
node --version
npm --version
```

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/DFE-Digital/sap-sector.git
cd sap-sector
```

**2. Restore .NET dependencies**

```bash
dotnet restore
```

**3. Install frontend dependencies**

```bash
cd SAPSec.Web
npm install
cd ..
```

`npm install` runs a `postinstall` hook that executes `npm run build-fe`, which uses Gulp to compile the Sass and JavaScript in `AssetSrc/` and copy GOV.UK Frontend and DfE Frontend into `wwwroot/lib/`. The contents of `wwwroot` are generated and are not committed — re-run this if assets look broken.

Convenience scripts at the repo root:

| Script | Purpose |
| --- | --- |
| `./build` / `build.bat` | Build frontend assets once |
| `./watch` / `watch.bat` | Run `dotnet watch` and the Gulp asset watcher together |

### User Secrets

The web app needs the following secrets locally. Values are provided by the team and **must never be committed**.

```bash
cd SAPSec.Web
dotnet user-secrets init

dotnet user-secrets set "DsiConfiguration:ClientId" "<value>"
dotnet user-secrets set "DsiConfiguration:ClientSecret" "<value>"
dotnet user-secrets set "ConnectionStrings:PostgresConnectionString" "<value>"
dotnet user-secrets set "LOGIT_HTTP_URL" "<value>"
dotnet user-secrets set "LOGIT_API_KEY" "<value>"

dotnet user-secrets list
```

### Database Setup

`SAPSec.Web` expects a local PostgreSQL database populated using the **SAPData** project.

1. **Install and start PostgreSQL** locally (or via Docker) and create an empty database.
2. **Download the source CSVs** from the sap-sector storage account `s189t01sapsecdptssa`, container `schooldata`, into `SAPData/DataMap/SourceFiles`. These files are large and reproducible from public sources, so they are deliberately git-ignored — do not commit them.
3. **Generate the SQL scripts:**

   ```bash
   cd SAPData
   dotnet run
   ```

4. **Run the generated scripts** from the SQL script directory:

   ```bash
   psql -d <DATABASE_NAME>
   ```

   ```sql
   \i run_all.sql
   ```

   If a script fails on encoding, re-save it as **UTF-8 without signature** (code page 65001) and re-run.

5. **Point the app at the database** via the `ConnectionStrings:PostgresConnectionString` user secret.

Full detail — including troubleshooting — is in [`docs/developers/002-dev-setup.md`](docs/developers/002-dev-setup.md) and [`SAPData/README.md`](SAPData/README.md).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Running Locally

### Option 1: .NET CLI

```bash
cd SAPSec.Web
dotnet run
```

The listening URLs are printed to the console on startup.

### Option 2: Visual Studio

1. Open `SAPSec.sln`
2. Set `SAPSec.Web` as the startup project
3. Press `F5` to run with debugging (or `Ctrl+F5` without)

### Option 3: VS Code

1. Open the repository folder
2. Press `F5` and select the .NET Core launch configuration

### Option 4: Watch mode (app + assets)

```bash
./watch          # macOS / Linux
watch.bat        # Windows
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Running with Docker

```bash
docker build -t sapsec:latest .
docker run -p 3000:3000 sapsec:latest
```

The application is then available at `http://localhost:3000`.

The multi-stage build:

1. **Assets stage** — Node 22 runs `npm ci --ignore-scripts` and `npm run build-fe`
2. **Build stage** — .NET 8 SDK restores, builds and publishes `SAPSec.Web`
3. **Runtime stage** — ASP.NET Core 8 runtime image, patched base packages, non-root user, listening on port 3000

> Note: a running container still needs a reachable PostgreSQL instance and DSI credentials supplied through environment variables.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Key Features

### DfE Sign-in authentication

Authentication uses OpenID Connect against DSI (`SAPSec.Web/Authentication/`). Authorization is applied through a **fallback policy requiring an authenticated user**, so every route is protected unless it opts out with `[AllowAnonymous]` (the home page, health check, error pages and static content do). Test environments swap DSI for an auto-authentication handler so integration, end-to-end and accessibility suites can run unattended.

### GOV.UK and DfE design system

- GOV.UK Frontend 5.x with the 2025 rebrand enabled via `AddGovUkFrontend(options => options.Rebrand = true)`
- DfE Frontend 2.x for departmental branding
- Breadcrumbs via SmartBreadcrumbs, styled with GOV.UK classes
- `en-GB` locale is enforced for all requests

### Search

School search is backed by **Lucene.NET**, registered through `AddLuceneDependencies()` and exposed behind interfaces so controllers never depend on Lucene directly. PostgreSQL remains the authoritative source of structured data; the Lucene index is derived, which means schema changes may require an index rebuild. See [`docs/developers/search-lucene.md`](docs/developers/search-lucene.md).

### Feature flags

`Microsoft.FeatureManagement` drives progressive rollout. The current flag is `FeatureManagement__EnablePrimarySchools`, enabled in review and test and disabled in production. See [`docs/developers/feature-management-strategy-aspnetcore-aks.md`](docs/developers/feature-management-strategy-aspnetcore-aks.md).

### Security headers and CSP

`SecurityHeadersMiddleware` applies a strict, nonce-based Content Security Policy, restricting external origins to the analytics providers in use. HSTS, HTTPS redirection, secure/HTTP-only cookies and antiforgery protection are all enabled outside development.

### Data protection

Data protection keys are configured in `Setup/DataProtectionSetup.cs` and persisted to Azure Blob Storage in deployed environments, so sessions and antiforgery tokens survive pod restarts and rolling deployments.

### Observability

- **Serilog** writes rendered compact JSON to the console, shipped to Logit in deployed environments
- **Sentry** captures exceptions, tagged per environment and disabled unless a DSN is present — see [`docs/operational/002-sentry-monitoring.md`](docs/operational/002-sentry-monitoring.md)
- **DfE Analytics** streams events to BigQuery using federated (workload identity) auth
- **Google Tag Manager** and **Microsoft Clarity** IDs are resolved per environment at startup

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Configuration

Configuration comes from `appsettings.json`, per-environment overrides (`appsettings.{Environment}.json`), user secrets locally, and Kubernetes ConfigMaps/Secrets in deployed environments. Nested keys use `__` as the separator in environment variables.

| Variable | Description | Source |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development`, `Test` or `Production` | `terraform/application/config/*.yml` |
| `ASPNETCORE_URLS` | Listening URLs (`http://+:3000` in Docker) | Dockerfile |
| `ENVIRONMENT_NAME` | Environment name used for analytics and Sentry tagging | ConfigMap |
| `ConnectionStrings__PostgresConnectionString` | .NET PostgreSQL connection string | Key Vault / user secrets |
| `DATABASE_URL` | PostgreSQL URL used by tooling | Terraform |
| `PGSSLMODE` | PostgreSQL SSL mode | Terraform |
| `DsiConfiguration__ClientId` / `__ClientSecret` / `__ApiSecret` | DfE Sign-in credentials | Key Vault / user secrets |
| `SENTRY_DSN` | Sentry DSN (Sentry stays off when unset) | Key Vault |
| `LOGIT_HTTP_URL` / `LOGIT_API_KEY` | Log shipping to Logit | Key Vault / user secrets |
| `StorageConnectionString` | Azure Storage for data protection keys | Terraform |
| `FeatureManagement__EnablePrimarySchools` | Toggles the primary school journeys | ConfigMap |
| `DfeAnalytics__Environment` / `__ProjectId` / `__DatasetId` / `__TableId` / `__CredentialsJson` | BigQuery analytics | Terraform / DfE Analytics module |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Routes and Endpoints

| Route | Description |
| --- | --- |
| `/` | Service home page (anonymous) |
| `/auth/signin`, `/auth/signout`, `/auth/signed-out` | DSI authentication journey |
| `/signin-oidc`, `/signout-callback-oidc` | OIDC callbacks |
| `/user/redirect` | Post sign-in routing based on the user's DSI organisation |
| `/find-a-school`, `/find-a-school/search`, `/find-a-school/suggest` | School search and autocomplete |
| `/school/primary/{urn}` | Primary school detail, plus `/ks2`, `/attendance`, `/school-details`, `/view-similar-schools`, `/what-is-a-similar-school` |
| `/school/primary/{urn}/view-similar-schools/{similarSchoolUrn}/…` | Primary similar-school comparison views |
| `/school/secondary/{urn}` | Secondary school detail, plus `/ks4-headline-measures`, `/ks4-core-subjects`, `/attendance`, `/school-details`, `/what-is-a-similar-school` |
| `/school/secondary/{urn}/view-similar-schools/{similarSchoolUrn}/…` | Secondary similar-school comparison views and their `/data` endpoints |
| `/ComparePerformance` | Compare performance landing page |
| `/accessibility`, `/terms-and-conditions`, `/cookies` | Static content |
| `/custom-event-tracking` | `POST` endpoint for analytics events |
| `/error/{statusCode}` | Error pages |
| `/health` | Detailed JSON health check (anonymous) |
| `/healthcheck` | Basic health probe used by AKS and deployments (anonymous) |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Health Checks

### `/healthcheck`

Registered through ASP.NET Core health checks and used by Kubernetes liveness/readiness probes and the deployment pipeline. Returns `Healthy` with a 200 status.

```bash
curl http://localhost:3000/healthcheck
```

### `/health`

A detailed JSON endpoint served by `HealthController`, intended for diagnostics.

```bash
curl http://localhost:3000/health | jq '.'
```

```json
{
  "status": "Healthy",
  "timestamp": "2026-08-20T10:30:00Z",
  "checks": [
    { "name": "ApplicationRunning", "status": "Pass", "message": "SAPSec.Web is running in Production environment" },
    { "name": "StaticFiles", "status": "Pass", "message": "Static files accessible: assets OK, CSS OK, libraries OK" }
  ]
}
```

Any failing check flips `status` to `Unhealthy` and the response to `500`. See [`docs/testing/002-health-check.md`](docs/testing/002-health-check.md).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Testing

### Run everything

```bash
dotnet test
```

### Run a single suite

```bash
dotnet test Tests/SAPSec.Core.Tests
dotnet test Tests/SAPSec.Infrastructure.Tests
dotnet test Tests/SAPSec.Test.Integration
dotnet test Tests/SAPSec.Test.EndToEnd
dotnet test Tests/SAPSec.Test.Accessibility
```

### Code coverage

```bash
dotnet test --settings coverlet.runsettings
```

`coverlet.runsettings` emits Cobertura, includes only `SAPSec.Web`, `SAPSec.Core` and `SAPSec.Infrastructure`, and excludes generated code, views and `Program.cs`. CI merges the per-project reports with `dotnet-coverage` and posts a coverage comment on the PR; a negative difference fails the build.

### Playwright (end-to-end and accessibility)

Install the browsers once:

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install --with-deps chromium
```

Tests run headless by default. To watch them run, set `HEADED=1` — attaching a debugger does this automatically:

```bash
HEADED=1 dotnet test Tests/SAPSec.Test.EndToEnd
```

In Visual Studio, use the Test Explorer as normal.

The wider strategy — unit, integration, HTTP, UI, accessibility, security, load and performance testing, plus who runs what and when — is documented in [`docs/testing/`](docs/testing/).

> `SAPSec.Web.Tests` and `SAPSec.UI.Tests` are marked **Deprecated** and are being replaced by the `SAPSec.Test.*` projects. Add new tests to the newer suites.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Data Pipeline

`SAPData` is a console application that turns official public school datasets into the curated PostgreSQL layer the service reads from. It:

1. downloads raw source files and computes hashes to detect change (exiting early when nothing has changed)
2. normalises and cleans the CSV inputs
3. generates deterministic SQL for raw tables and loading
4. builds staging, dimension and fact tables, views and similar-school groupings
5. applies indexes and validation checks

The pipeline runs through the **School Data Ingestion Pipeline** workflow (`data-pipeline.yml`), which can target test or production and can also produce a seed backup for a review app. Raw data files are never committed. See [`SAPData/README.md`](SAPData/README.md) and [`docs/data/`](docs/data/).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Deployment

### Review apps

Review apps give each pull request an isolated, production-like environment.

1. Open your PR and add the **`deploy`** label.
2. GitHub Actions builds the image, deploys to the `sip-development` namespace on the test cluster, and runs the `/healthcheck` probe. Expect roughly 5–10 minutes.
3. The environment URL is published to the PR's **Environments** section and the workflow summary.
4. Pushing new commits redeploys automatically — no need to re-add the label.
5. The review app is deleted automatically when the PR is closed or merged (`delete-review-app.yml`). A weekly reconcile job (`review-app-reconcile.yml`) cleans up anything stale.

Review app databases are reset and restored from a seed backup when `reset_review_db` is `true` in `terraform/application/config/review.yml`.

### Continuous deployment

```
PR opened ──► build image (+ Snyk scan) ──► run all tests with coverage
     │
     ├── 'deploy' label ──► review app ──► health check ──► URL on PR
     │
     └── merged to main ──► deploy to test AND production ──► health checks ──► domains + Front Door
```

Pushing to `main` runs the deploy job as a matrix across **test and production**. Deployments can also be triggered manually via `workflow_dispatch`, choosing the environment, image tag and (for review) the PR number.

### Workflows

| Workflow | Purpose |
| --- | --- |
| `build-and-deploy.yml` | Build, test, coverage, review apps, deploy to test/production, deploy domains |
| `build-nocache.yml` | Weekly cache-free image build to surface stale-cache problems |
| `delete-review-app.yml` | Tears down a review app when its PR closes |
| `review-app-reconcile.yml` | Weekly sweep for orphaned review apps |
| `data-pipeline.yml` | School data ingestion and curated layer rebuild |
| `backup-db.yml` | Nightly (04:00 UTC) and ad-hoc database backups to Azure Storage |
| `postgres-restore.yml` | Restore a database from a backup in Azure Storage |
| `postgres-ptr.yml` | Point-in-time restore to a new database server |
| `restore-deleted-postgres.yml` | Recover a deleted PostgreSQL server |
| `toggle-maintenance-page.yml` | Route traffic to or away from the maintenance page |
| `validate-infrastructure.yml` | Daily Terraform validation |

### Maintenance page

A standalone nginx image in `maintenance_page/` is built alongside the application and deployed to each environment. Traffic is switched over with the **Manage Website Maintenance Mode** workflow, or locally:

```bash
make test enable-maintenance
make test disable-maintenance
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Infrastructure

Infrastructure is Terraform, deployed to Teacher Services Cloud AKS and driven through the `Makefile`. Per-environment Azure settings live in `global_config/`, and Terraform variables in `terraform/application/config/`.

| Environment | Cluster | Namespace | Subscription |
| --- | --- | --- | --- |
| Review | test | `sip-development` | `s189-teacher-services-cloud-test` |
| Test | test | `sip-test` | `s189-teacher-services-cloud-test` |
| Production | production | `sip-production` | `s189-teacher-services-cloud-production` |

Common commands (requires Azure CLI login):

```bash
make help                                   # list documented targets
make test terraform-plan                    # plan the test environment
make test terraform-apply                   # apply the test environment
make production CONFIRM_PRODUCTION=yes terraform-plan
make review PR_NUMBER=123 terraform-plan    # plan a review app
make domains-infra-plan                     # DNS zone and Front Door
make test domains-plan                      # environment domains
make test get-cluster-credentials           # kubectl access
make test show-service                      # inspect the running service
make test scale-app REPLICAS=2
```

Production runs two replicas with PostgreSQL high availability, backup storage and monitoring enabled; test runs two replicas with backup storage. DNS and CDN are managed under `terraform/domains/`, fronted by Azure Front Door with `/assets/*` cached and rate limiting applied.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Documentation

| Area | Contents |
| --- | --- |
| [`docs/architecture/`](docs/architecture/) | High-level and low-level design, entity relationship diagram |
| [`docs/adrs/`](docs/adrs/) | 15 architecture decision records — service name, branching, language, data storage, hosting, secrets, auth, testing, progressive enhancement and more |
| [`docs/developers/`](docs/developers/) | Dev setup, git flow, project structure, coding standards, frontend, backend, authentication, security, search, testing, feature management |
| [`docs/testing/`](docs/testing/) | Testing strategy, health checks, unit, UI, HTTP, accessibility, security, load and performance testing, execution model |
| [`docs/operational/`](docs/operational/) | Runbooks, Sentry monitoring |
| [`docs/data/`](docs/data/) · [`SAPData/README.md`](SAPData/README.md) | Data platform and ingestion pipeline |
| [`docs/analysis/`](docs/analysis/) | Analysis notes and data pipeline pain points |
| [`docs/_templates/`](docs/_templates/) | Templates for new documentation and ADRs |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

Contributions are welcome. `main` is always deployable, so all work happens on a branch and lands through a reviewed pull request.

### Branching

Branch names follow the convention in [`docs/developers/003-git-flow.md`](docs/developers/003-git-flow.md):

```
feature/{trello-id}-{short-description}    e.g. feature/1001-let-users-login-with-azure-credentials
bug/{trello-id}-{short-description}        e.g. bug/1002-azure-login-not-working-for-scunthorpe-la
```

### Workflow

1. Branch from `main`.
2. Make your change, following the conventions in [`docs/developers/coding-standards.md`](docs/developers/coding-standards.md). Keep controllers thin, put business logic in `SAPSec.Core` and data access in `SAPSec.Infrastructure`.
3. Add or update tests where behaviour changes, and update documentation where developer behaviour changes.
4. Verify locally:

   ```bash
   dotnet build
   dotnet test
   dotnet run --project SAPSec.Web
   ```

5. Open a pull request and complete the [PR template](.github/PULL_REQUEST_TEMPLATE.MD) — description, Trello link, type of change, how it was tested, checklist and screenshots for UI changes.
6. Optionally add the `deploy` label to spin up a review app for reviewers and stakeholders.
7. Get at least one approval and ensure all CI checks pass (build, tests, coverage, Snyk).
8. Squash and merge, then delete the branch. Merging to `main` deploys to test and production.

Keep pull requests small and focused, and prefer consistency with the existing patterns over introducing new ones.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under the MIT License — Crown Copyright (Department for Education). See [`license.md`](license.md) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

- **Issues:** [GitHub Issues](https://github.com/DFE-Digital/sap-sector/issues)
- **Project Link:** [https://github.com/DFE-Digital/sap-sector](https://github.com/DFE-Digital/sap-sector)
- **Related repository:** [DFE-Digital/sap-public](https://github.com/DFE-Digital/sap-public) — the public facing output of the SAP project

When reporting a bug, please include the environment (local/review/test/production), steps to reproduce, expected and actual behaviour, screenshots for UI issues, and your browser and OS.

### Useful links

- [DfE Technical Guidance](https://technical-guidance.education.gov.uk/)
- [GOV.UK Design System](https://design-system.service.gov.uk/)
- [DfE Design Manual](https://design.education.gov.uk/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Azure Kubernetes Service](https://docs.microsoft.com/en-us/azure/aks/)
- [Teacher Services Cloud](https://github.com/DFE-Digital/teacher-services-cloud)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

**Maintained by:** DfE Digital — SAP Sector Team

<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/DFE-Digital/sap-sector.svg?style=for-the-badge
[contributors-url]: https://github.com/DFE-Digital/sap-sector/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/DFE-Digital/sap-sector.svg?style=for-the-badge
[forks-url]: https://github.com/DFE-Digital/sap-sector/network/members
[stars-shield]: https://img.shields.io/github/stars/DFE-Digital/sap-sector.svg?style=for-the-badge
[stars-url]: https://github.com/DFE-Digital/sap-sector/stargazers
[issues-shield]: https://img.shields.io/github/issues/DFE-Digital/sap-sector.svg?style=for-the-badge
[issues-url]: https://github.com/DFE-Digital/sap-sector/issues
[license-shield]: https://img.shields.io/badge/License-MIT-Yellow.svg?style=for-the-badge
[license-url]: https://github.com/DFE-Digital/sap-sector/blob/main/license.md
