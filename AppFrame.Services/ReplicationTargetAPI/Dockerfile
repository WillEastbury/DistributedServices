# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build app
COPY . .
RUN dotnet publish -c release -o /app

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ReplicationTargetAPI.dll"]
