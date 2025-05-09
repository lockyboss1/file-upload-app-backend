# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy solution file
COPY file-upload-app-backend.sln .

# Copy all projects
COPY Domain/ Domain/
COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY Tests/ Tests/
COPY file-upload-app-backend/ file-upload-app-backend/

# Restore dependencies using the solution file
RUN dotnet restore file-upload-app-backend.sln

# Build the entire solution
RUN dotnet build --no-restore -c Release

# Publish the API project
RUN dotnet publish file-upload-app-backend/file-upload-app-backend.csproj -c Release -o /app/publish --no-restore

# Run tests
RUN dotnet test

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Set environment variable for ASP.NET Core to listen on all network interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "file-upload-app-backend.dll"]
