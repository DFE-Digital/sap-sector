# -----------------------------
# Stage 1: .NET build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects first (for better caching)
COPY SAPSec.sln ./
COPY SAPSec.Core/SAPSec.Core.csproj SAPSec.Core/
COPY SAPSec.Infrastructure/SAPSec.Infrastructure.csproj SAPSec.Infrastructure/
COPY SAPSec.Web/SAPSec.Web.csproj SAPSec.Web/
COPY Tests/Tests.csproj Tests/

# Restore dependencies
RUN dotnet restore SAPSec.sln

# Copy rest of the source
COPY . .

# Publish (Release build)
RUN dotnet publish SAPSec.Web/SAPSec.Web.csproj -c Release -o /app/publish

# -----------------------------
# Stage 2: Frontend build
# -----------------------------
FROM node:18-alpine AS frontend
WORKDIR /app

# Copy only package.json/package-lock.json first
COPY SAPSec.Web/package*.json ./SAPSec.Web/

WORKDIR /app/SAPSec.Web
RUN npm ci

# Copy rest of frontend source
COPY SAPSec.Web/ ./

# Build frontend assets
RUN npm run build

# -----------------------------
# Stage 3: Final runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published .NET app
COPY --from=build /app/publish .

# Copy built frontend assets into wwwroot (adjust path if different)
COPY --from=frontend /app/SAPSec.Web/wwwroot ./wwwroot

# Expose port
EXPOSE 8080

# Run app
ENTRYPOINT ["dotnet", "SAPSec.Web.dll"]
