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

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "file-upload-app-backend.dll"]
