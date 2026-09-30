FROM mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.24 AS build
WORKDIR /source

COPY global.json NuGet.Config Directory.Build.props Directory.Build.targets Directory.Packages.props BannedSymbols.txt .editorconfig config.nsdepcop ./
COPY src/SpecFirst.Service.Api/SpecFirst.Service.Api.csproj src/SpecFirst.Service.Api/
COPY src/SpecFirst.Service.Messages/SpecFirst.Service.Messages.csproj src/SpecFirst.Service.Messages/
COPY src/SpecFirst.Service.Models/SpecFirst.Service.Models.csproj src/SpecFirst.Service.Models/
RUN dotnet restore src/SpecFirst.Service.Api

COPY src/ src/
RUN dotnet publish src/SpecFirst.Service.Api -c Release --no-restore -o /app /p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12-alpine3.24
RUN apk add --no-cache krb5-libs
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -T 2 -O /dev/null http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "SpecFirst.Service.Api.dll"]
