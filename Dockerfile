# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Create a writable folder for Data Protection keys
RUN mkdir /keys && chmod -R 777 /keys

# Tell ASP.NET Core to listen on port 3000 (to match AKS probe)
ENV ASPNETCORE_URLS=http://+:3000

# Expose port 3000 (AKS health probes target this)
EXPOSE 3000

# Important: switch to non-root user *after* permissions are set
USER $APP_UID


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SAPSec.Web/SAPSec.Web.csproj", "SAPSec.Web/"]
COPY ["SAPSec.Core/SAPSec.Core.csproj", "SAPSec.Core/"]
COPY ["SAPSec.Infrastructure/SAPSec.Infrastructure.csproj", "SAPSec.Infrastructure/"]
RUN dotnet restore "./SAPSec.Web/SAPSec.Web.csproj"

COPY . .
WORKDIR "/src/SAPSec.Web"
RUN dotnet build "./SAPSec.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SAPSec.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SAPSec.Web.dll"]
