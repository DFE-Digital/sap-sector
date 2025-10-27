# =====================================================
# Stage 0: Set version arguments
# =====================================================
ARG DOTNET_VERSION=8.0
ARG NODEJS_VERSION_MAJOR=22

# =====================================================
# Stage 1: Build frontend assets (DfE / GOV.UK)
# =====================================================
FROM node:${NODEJS_VERSION_MAJOR}-bookworm-slim AS assets
WORKDIR /app

COPY ./SAPSec.Web/package*.json /app/

RUN npm ci

COPY ./SAPSec.Web/wwwroot/ /app/wwwroot/

RUN echo "=== Assets build output ===" && \
    echo "Checking wwwroot structure:" && \
    find /app/wwwroot -type d | head -20 && \
    echo "=== Checking for frontend libraries ===" && \
    ls -la /app/wwwroot/lib/ 2>/dev/null || echo "No lib directory yet" && \
    ls -la /app/wwwroot/lib/dfe-frontend/ 2>/dev/null || echo "DfE frontend not in wwwroot/lib" && \
    ls -la /app/wwwroot/lib/govuk-frontend/ 2>/dev/null || echo "GOV.UK frontend not in wwwroot/lib"

# =====================================================
# Stage 2: Build .NET project
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SAPSec.Web/SAPSec.Web.csproj", "SAPSec.Web/"]
COPY ["SAPSec.Core/SAPSec.Core.csproj", "SAPSec.Core/"]
COPY ["SAPSec.Infrastructure/SAPSec.Infrastructure.csproj", "SAPSec.Infrastructure/"]
RUN dotnet restore "./SAPSec.Web/SAPSec.Web.csproj"

COPY . .

WORKDIR "/src/SAPSec.Web"
RUN dotnet build "./SAPSec.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SAPSec.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =====================================================
# Stage 3: Runtime image
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble AS final
WORKDIR /app

RUN apt-get update && \
    apt-get upgrade -y zlib1g libpam0g libpam-modules libpam-modules-bin libpam-runtime && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN mkdir -p /keys && chmod -R 777 /keys

ENV ASPNETCORE_URLS=http://+:3000

COPY --from=publish /app/publish .

COPY --from=assets /app/wwwroot ./wwwroot

RUN echo "=== Final wwwroot contents ===" && \
    ls -la /app/wwwroot/ && \
    echo "=== Checking for frontend libraries ===" && \
    ls -la /app/wwwroot/lib/ 2>/dev/null || echo "No lib directory" && \
    echo "=== Checking for DfE frontend ===" && \
    ls -la /app/wwwroot/lib/dfe-frontend/ 2>/dev/null || echo "DfE not found"

EXPOSE 3000

USER $APP_UID

ENTRYPOINT ["dotnet", "SAPSec.Web.dll"]