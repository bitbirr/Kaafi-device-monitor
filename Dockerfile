# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Kaafi.DeviceMonitor.csproj", "./"]
RUN dotnet restore "Kaafi.DeviceMonitor.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Kaafi.DeviceMonitor.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Kaafi.DeviceMonitor.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kaafi.DeviceMonitor.dll"]
