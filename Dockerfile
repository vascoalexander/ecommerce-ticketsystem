# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/WebApp/*.csproj ./WebApp/
RUN dotnet restore ./WebApp/WebApp.csproj

COPY src/WebApp/. ./WebApp/
WORKDIR /src/WebApp
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]