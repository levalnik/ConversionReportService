FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish src/ConversionReportService/ConversionReportService.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "ConversionReportService.dll"]
