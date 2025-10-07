# =====================================================
# Stage 0: Set version arguments
# =====================================================
ARG DOTNET_VERSION=8.0
ARG NODEJS_VERSION_MAJOR=22


# =====================================================
# Stage 1: Build frontend assets (DfE / GOV.UK)
# =====================================================
FROM node:${NODEJS_VERSION_MAJOR}-bullseye-slim AS assets
WORKDIR /app

# Copy package files for dependency installation
COPY ./SAPSec.Web/package*.json /app/

# Install dependencies - this will trigger postinstall which runs copy-assets
# The postinstall script copies dfe-frontend and govuk-frontend from node_modules to wwwroot/lib
RUN npm ci

# Copy all wwwroot contents (custom assets, images, CSS, etc.)
# The .dockerignore should exclude wwwroot/lib if it exists locally
COPY ./SAPSec.Web/wwwroot/ /app/wwwroot/

# Debug: Show what was built and where
RUN echo "=== Assets build output ===" && \
    echo "Checking wwwroot structure:" && \
    find /app/wwwroot -type d | head -20 && \
    echo "=== Checking for frontend libraries ===" && \
    ls -la /app/wwwroot/lib/ 2>/dev/null || echo "No lib directory yet" && \
    ls -la /app/wwwroot/lib/dfe-frontend/dist/ 2>/dev/null || echo "DfE frontend not in wwwroot/lib" && \
    ls -la /app/wwwroot/lib/govuk-frontend/dist/ 2>/dev/null || echo "GOV.UK frontend not in wwwroot/lib"


# =====================================================
# Stage 2: Build .NET project
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-azurelinux3.0 AS build
WORKDIR /build

# Copy project files and restore packages
COPY ./SAPSec.Core/SAPSec.Core.csproj /build/SAPSec.Core/
COPY ./SAPSec.Infrastructure/SAPSec.Infrastructure.csproj /build/SAPSec.Infrastructure/
COPY ./SAPSec.Web/SAPSec.Web.csproj /build/SAPSec.Web/
RUN dotnet restore /build/SAPSec.Web/SAPSec.Web.csproj

# Copy all code
COPY . .

# Build and publish .NET project
WORKDIR /build/SAPSec.Web
RUN dotnet build -c Release --no-restore && \
    dotnet publish -c Release --no-build -o /app/publish /p:UseAppHost=false


# =====================================================
# Stage 3: Runtime image
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-azurelinux3.0 AS final
WORKDIR /app

# Environment configuration
ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_ENVIRONMENT=Production

# Create directories with proper permissions for data protection keys
RUN mkdir -p /keys /home/app/.aspnet/DataProtection-Keys && \
    chmod -R 777 /keys /home/app/.aspnet

# Copy published .NET app first
COPY --from=build /app/publish /app

# Copy frontend assets (will overwrite/merge with published wwwroot)
COPY --from=assets /app/wwwroot /app/wwwroot

# Debug: Verify static files are present with correct permissions
RUN echo "=== Final wwwroot contents ===" && \
    ls -laR /app/wwwroot/ | head -50 && \
    echo "=== Checking for CSS files ===" && \
    find /app -name "*.css" -type f -exec ls -lh {} \;

# Azure Linux images have a default non-root user (ID 1654)
# Ensure proper ownership
RUN chown -R 1654:1654 /app /keys /home/app

# Switch to non-root user
USER 1654

# Expose port
EXPOSE 3000

# Entry point
ENTRYPOINT ["dotnet", "SAPSec.Web.dll"]