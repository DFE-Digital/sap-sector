# SAP Sector - School Profile (Sector Facing)

[![Build and Deploy](https://github.com/DFE-Digital/sap-sector/actions/workflows/build-and-deploy.yml/badge.svg)](https://github.com/DFE-Digital/sap-sector/actions/workflows/build-and-deploy.yml)

Sector facing output of the SAP (School Account Profile) project. This service allows schools to manage and update their profile information.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running Locally](#running-locally)
- [Running with Docker](#running-with-docker)
- [Project Structure](#project-structure)
- [Key Features](#key-features)
- [Health Checks](#health-checks)
- [Environment Variables](#environment-variables)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [Support](#support)

## Overview

The SAP Sector service is a web application built with ASP.NET Core 8.0 that provides a user interface for schools to:
- Schools can view and comapare with similar schools
- Schools should use DSI Signing in to authenticate
- 
The application uses the GOV.UK Design System and DfE Frontend for a consistent, accessible user experience following government design standards.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22.x](https://nodejs.org/) (for building frontend assets)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (recommended)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/DFE-Digital/sap-sector.git
cd sap-sector
```

### 2. Install .NET dependencies

```bash
dotnet restore
```

### 3. Install Node.js dependencies (for frontend assets)

```bash
cd SAPSec.Web
npm install
cd ..
```

The `npm install` command automatically runs a `postinstall` script that copies GOV.UK Frontend and DfE Frontend libraries from `node_modules` to `wwwroot/lib/`.

## Running Locally

### Option 1: Using .NET CLI

```bash
cd SAPSec.Web
dotnet run
```

The application will be available at `http://localhost:3000`

### Option 2: Using Visual Studio

1. Open `sap-sector.sln` in Visual Studio
2. Press `F5` to run with debugging (or `Ctrl+F5` without debugging)
3. The application will launch in your default browser

### Option 3: Using VS Code

1. Open the project folder in VS Code
2. Press `F5` to start debugging
3. Select ".NET Core Launch (web)" configuration
4. Navigate to `http://localhost:3000`

## Running with Docker

### Build the Docker image

```bash
docker build -t sapsec:latest .
```

The Docker build process:
1. **Assets stage**: Builds frontend assets using Node.js
2. **Build stage**: Compiles .NET application
3. **Final stage**: Creates minimal runtime image

### Run the container

```bash
docker run -p 3000:3000 sapsec:latest
```

The application will be available at `http://localhost:3000`

## Project Structure

```
sap-sector/
├── .github/
│   └── workflows/              # GitHub Actions CI/CD pipelines
│       ├── build-and-deploy.yml
│       ├── build-nocache.yml
│       └── delete-review-app.yml
├── SAPSec.Core/               # Domain models and business logic
│   └── SAPSec.Core.csproj
├── SAPSec.Infrastructure/     # Data access and external services
│   ├── Data/                  # Database context (future)
│   └── SAPSec.Infrastructure.csproj
├── SAPSec.Web/               # Web application (MVC)
│   ├── Controllers/          # MVC Controllers
│   │   ├── HomeController.cs
│   │   └── HealthController.cs
│   ├── Views/               # Razor views
│   │   ├── Home/
│   │   └── Shared/
│   ├── wwwroot/             # Static files
│   │   ├── assets/          # Custom assets (images)
│   │   ├── css/            # Custom stylesheets
│   │   └── lib/            # Frontend libraries (auto-generated)
│   ├── Helpers/            # Helper classes
│   ├── package.json        # Node.js dependencies
│   └── Program.cs         # Application entry point
├── terraform/
│   └── application/        # Terraform infrastructure
├── Dockerfile             # Docker configuration
├── .dockerignore         # Docker ignore patterns
├── .gitignore           # Git ignore patterns
└── README.md           # This file
```

## Key Features

### GOV.UK Design System Integration

The application uses:
- **GOV.UK Frontend v5.12.0** - Standard GOV.UK design system components
- **DfE Frontend v2.0.1** - Department for Education branding and components

Frontend assets are automatically managed:
- Installed via npm from official packages
- Copied to `wwwroot/lib/` during `npm install` via postinstall script
- Not committed to Git (regenerated during Docker build)

### DfE Branded Header

Custom DfE header with:
- Department for Education branding
- Service name display
- Alpha phase banner
- Mobile-responsive design

### Content Security Policy (CSP)

The application implements strict Content Security Policy headers for security:
- Nonce-based script execution
- Restricted external domains (Google Analytics, Microsoft Clarity, Application Insights)
- Protection against XSS and injection attacks
- Font and style restrictions

### Data Protection

Keys are persisted to `/keys` directory for:
- Session management
- Anti-forgery tokens
- Data protection across deployments

## Health Checks

The application provides two health check endpoints for monitoring:

### `/healthcheck` - Basic Health Check

Simple endpoint used by Kubernetes liveness/readiness probes and the deployment pipeline.

**Response (200 OK):**
```
Healthy
```

**Usage:**
```bash
curl http://localhost:3000/healthcheck
```

### `/health` - Detailed Health Check

Comprehensive endpoint providing detailed status information for monitoring and diagnostics.

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-15T10:30:00Z",
  "checks": [
    {
      "name": "ApplicationRunning",
      "status": "Pass",
      "message": "SAPSec.Web is running in Production environment"
    },
    {
      "name": "StaticFiles",
      "status": "Pass",
      "message": "Static files accessible: assets OK, CSS OK, libraries OK"
    }
  ]
}
```

**Response (500 Internal Server Error):**
```json
{
  "status": "Unhealthy",
  "timestamp": "2025-01-15T10:30:00Z",
  "checks": [
    {
      "name": "StaticFiles",
      "status": "Fail",
      "message": "wwwroot directory not found"
    }
  ]
}
```

**Usage:**
```bash
# Check health status
curl http://localhost:3000/health

# Check and pretty-print JSON
curl http://localhost:3000/health | jq '.'
```

**Checks performed:**
- ✅ Application is running
- ✅ Static files exist and are accessible
- 💤 Database connectivity (ready to enable when database is added)

## Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` | No |
| `ASPNETCORE_URLS` | URLs the application listens on | `http://+:3000` | No |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Enable forwarded headers for proxies | `true` | For AKS |

## API Endpoints

| Endpoint | Method | Description | Response |
|----------|--------|-------------|----------|
| `/` | GET | Home page | HTML |
| `/health` | GET | Detailed health check | JSON |
| `/healthcheck` | GET | Basic health check | Text |
| `/Home/Privacy` | GET | Privacy policy page | HTML |
| `/Home/Error` | GET | Error page | HTML |

## Testing

### Run unit tests

```bash
dotnet test
```

### Run with code coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReporter=opencover
```

### Manual testing

```bash
# Start the application
dotnet run --project SAPSec.Web

# In another terminal, test the health endpoint
curl http://localhost:3000/health
```

## Deployment

### Review Apps (PR Environments)

Review apps provide temporary, isolated environments for testing pull requests before merging.

#### How Review Apps Work

1. **Triggering Deployment:**
   - Open your pull request on GitHub
   - Add the `deploy` label from the labels menu
   - GitHub Actions automatically builds and deploys your branch

2. **Deployment Process:**
   ```
   Add 'deploy' label → Build Docker image → Deploy to AKS → Health checks → Comment with URL
   ```
   
   Duration: ~5-10 minutes

3. **Accessing Your Review App:**
   - **URL Format:** `https://sap-sector-{NUMBER}.test.education.gov.uk`
   - **Example:** For PR #42: `https://sap-sector-42.test.education.gov.uk`
   - Find the URL in:
     - PR comments (posted by GitHub Actions)
     - "Environments" section on the PR page
     - GitHub Actions workflow logs

4. **Testing in Review App:**
   ```bash
   # Replace {PR_NUMBER} with your actual PR number
   export REVIEW_URL="https://sap-sector-{PR_NUMBER}.test.education.gov.uk"
   
   # Test the application
   curl $REVIEW_URL
   
   # Check health endpoints
   curl $REVIEW_URL/health | jq '.'
   curl $REVIEW_URL/healthcheck
   ```

5. **Updating the Review App:**
   - Push new commits to your PR branch
   - The review app automatically redeploys with your latest changes
   - No need to remove/re-add the label

6. **Sharing with Stakeholders:**
   - Copy the review app URL from PR comments
   - Share with team members, testers, or stakeholders
   - Everyone can test the changes before merging

7. **Cleanup:**
   - Review app is **automatically deleted** when:
     - PR is closed
     - PR is merged
     - `deploy` label is removed
   - No manual cleanup required

#### Review App Benefits

✅ **Test in production-like environment** before merging  
✅ **Share live preview** with non-technical stakeholders  
✅ **Verify integrations** with external services  
✅ **Check responsive design** on real devices  
✅ **Validate infrastructure changes** safely  
✅ **Automatic cleanup** - no manual management  

#### Troubleshooting Review Apps

**Deployment failed:**
- Check GitHub Actions logs for errors
- Verify Docker build succeeded
- Ensure all tests pass
- Check health check endpoint returns 200

**Can't access review app URL:**
- Wait 5-10 minutes for initial deployment
- Check PR comments for the correct URL
- Verify `deploy` label is applied
- Check GitHub Actions workflow completed successfully

**Changes not appearing:**
- Push a new commit to trigger redeployment
- Check GitHub Actions shows deployment completed
- Clear browser cache and refresh

### GitHub Actions CI/CD

The project uses GitHub Actions for automated deployment. Workflows are located in `.github/workflows/`:

#### Deployment Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ Developer pushes to feature branch                              │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│ Creates Pull Request                                            │
│ • CI builds and tests automatically run                         │
│ • Docker image is built and cached                              │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│ Add 'deploy' label (Optional)                                   │
│ • Triggers deployment to review environment                     │
│ • Creates temporary URL for testing                             │
│ • Runs health checks                                            │
│ • Posts URL in PR comments                                      │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│ PR Approved & Merged to main                                    │
│ • Automatic deployment to test environment                      │
│ • Health checks verify deployment                               │                                     │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│ Manual promotion to production (Future)                         │
└─────────────────────────────────────────────────────────────────┘
```

#### Workflows

##### `build-and-deploy.yml`
Main deployment workflow that handles:

**On Pull Request:**
- ✅ Builds Docker image
- ✅ Runs unit tests
- ✅ Caches build artifacts
- ✅ Runs security scan with Snyk
- ✅ *If `deploy` label added:* Deploys to review environment

**On Push to main:**
- ✅ Builds production Docker image
- ✅ Runs all tests
- ✅ Deploys to test environment
- ✅ Runs health checks (`/healthcheck` endpoint)

**Health Check Verification:**
```yaml
# The deploy-to-aks action automatically:
1. Deploys the application
2. Waits for pods to be ready
3. Checks /healthcheck endpoint
4. Retries if unhealthy (up to 5 times)
5. Fails deployment if health check doesn't pass
```

##### `build-nocache.yml`
Builds Docker image without using cache (for troubleshooting build issues)

##### `delete-review-app.yml`
Automatically cleans up review environments when PRs are closed or merged

### Deployment Environments

| Environment | Trigger | URL |
|------------|---------|-----|
| **Review** | PR with `deploy` label | Dynamic per PR |
| **Test** | Push to `main` | TBD |
| **Production** | Manual approval | TBD |

### Azure Kubernetes Service (AKS)

The application is deployed to Azure Kubernetes Service with:
- Health probes configured for `/healthcheck` endpoint
- Auto-scaling based on CPU/memory metrics
- Horizontal pod autoscaling (HPA)
- Non-root container execution (user 1654)
- Data protection keys persisted to `/keys`

### Infrastructure

Infrastructure is managed with Terraform:
- Configuration located in `terraform/application/`
- Deployed via GitHub Actions
- Includes AKS cluster, networking, and monitoring

## Contributing

### Development Workflow

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes:**
   - Write code following C# conventions
   - Add/update tests
   - Update documentation

3. **Test locally:**
   ```bash
   dotnet build
   dotnet test
   dotnet run --project SAPSec.Web
   
   # Test health endpoints
   curl http://localhost:3000/health
   curl http://localhost:3000/healthcheck
   ```

4. **Commit your changes:**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

5. **Push to GitHub:**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request:**
   - Go to https://github.com/DFE-Digital/sap-sector/pulls
   - Click "New Pull Request"
   - Select your branch as the compare branch
   - Add a clear title and description of changes
   - Request reviews from team members

7. **Deploy to Review Environment (Optional):**
   
   To test your changes in a live environment before merging:
   
   a. **Add the `deploy` label to your PR:**
      - On your PR page, click "Labels" on the right sidebar
      - Select or type `deploy`
      - Click outside the dropdown to apply
   
   b. **GitHub Actions will automatically:**
      - Build a Docker image with your changes
      - Deploy to a temporary review environment in AKS
      - Run health checks to verify the deployment
      - Add a comment to your PR with the review app URL
   
   c. **Access your review app:**
      - Look for the environment URL in the PR comments
      - Format: `https://sap-sector-review-pr-{PR_NUMBER}.test.education.gov.uk`
      - Example: `https://sap-sector-review-pr-123.test.education.gov.uk`
   
   d. **Verify your changes:**
      - Test all functionality in the review environment
      - Check that UI changes appear correctly
      - Verify health endpoints work: `{URL}/health` and `{URL}/healthcheck`
      - Share the URL with reviewers for testing
   
   e. **Review app lifecycle:**
      - Created automatically when `deploy` label is added
      - Updated automatically on each new push to the PR branch
      - Deleted automatically when the PR is closed or merged

8. **Get Approval and Merge:**
   - Address any review comments
   - Ensure all CI checks pass (build, tests, health checks)
   - Get at least one approval
   - Merge the PR (squash and merge recommended)

9. **Automatic Deployment:**
   - After merging to `main`, changes automatically deploy to `test` environment
   - Monitor the deployment in GitHub Actions
   - Verify health checks pass in test environment

### Code Style

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Write unit tests for new functionality
- Keep methods small and focused (Single Responsibility Principle)

### Commit Message Convention

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, no logic change)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

**Examples:**
```
feat(health): add database connectivity check
fix(auth): resolve session timeout issue
docs: update deployment instructions in README
test(controllers): add unit tests for HealthController
```

### Pull Request Guidelines

#### Before Creating a PR

- Keep PRs focused and small (ideally < 400 lines changed)
- Write clear, descriptive titles using conventional commits format
- Test your changes locally
- Ensure all tests pass: `dotnet test`
- Check code builds without errors: `dotnet build`

#### Creating the PR

1. **Title Format:**
   ```
   <type>(<scope>): <description>
   
   Examples:
   feat(health): add database connectivity check
   fix(auth): resolve session timeout issue
   docs: update deployment instructions
   ```

2. **Description Template:**
   ```markdown
   ## What
   Brief description of what this PR does
   
   ## Why
   Explanation of why this change is needed
   
   ## How to Test
   Steps to verify the changes:
   1. Step one
   2. Step two
   3. Expected result
   
   ## Screenshots (if applicable)
   [Add screenshots of UI changes]
   
   ## Checklist
   - [ ] Tests added/updated
   - [ ] Documentation updated
   - [ ] Tested locally
   - [ ] Ready for review
   ```

#### Review Environment Deployment

**When to use review apps:**
- Testing UI changes with stakeholders
- Verifying integration with external services
- Demonstrating new features
- Testing in a production-like environment

**How to deploy:**
1. Add `deploy` label to your PR
2. Wait for GitHub Actions to complete (~5-10 minutes)
3. Find the review app URL in PR comments or environments tab
4. Share URL with reviewers for testing

**Important notes:**
- Review apps are temporary (deleted when PR closes)
- Each PR gets its own isolated environment
- Changes are automatically redeployed on every push
- Review apps use the same configuration as test environment

#### Review Process

- **Minimum 1 approval required** before merging
- All CI checks must pass (build, tests, health checks)
- Address all review comments or mark them as resolved
- Keep the PR updated by pulling latest changes from main
- Use "Request changes" for blocking issues
- Use "Comment" for non-blocking suggestions

#### After Approval

- Use **"Squash and merge"** (preferred) to keep history clean
- Ensure commit message follows conventional commits
- Delete the branch after merging
- Monitor deployment to test environment

## Support

### Getting Help

- **Slack:** `#sap-sector-support` (DfE Digital Slack workspace)
- **Email:** [sap-sector-team@education.gov.uk](mailto:sap-sector-team@education.gov.uk)
- **Issues:** [GitHub Issues](https://github.com/DFE-Digital/sap-sector/issues)

### Reporting Issues

When reporting bugs, please include:
1. Environment (local/review/test/production)
2. Steps to reproduce
3. Expected behavior
4. Actual behavior
5. Screenshots (if applicable)
6. Browser/OS information (for UI issues)

## Useful Links

- [DfE Technical Guidance](https://technical-guidance.education.gov.uk/)
- [GOV.UK Design System](https://design-system.service.gov.uk/)
- [DfE Design Manual](https://design.education.gov.uk/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Azure Kubernetes Service](https://docs.microsoft.com/en-us/azure/aks/)
- [GitHub Actions](https://docs.github.com/en/actions)

## License

[MIT License](LICENSE) - Crown Copyright (Department for Education)

---

**Maintained by:** DfE Digital - SAP Sector Team  
**Last Updated:** January 2025