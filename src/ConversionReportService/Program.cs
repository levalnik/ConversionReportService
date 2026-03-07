using ConversionReportService.Application.Extensions;
using ConversionReportService.Infrastructure.DataAccess.Extensions;
using ConversionReportService.Presentation.Grpc.Extensions;
using ConversionReportService.Presentation.Kafka.Extensions;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddDatabaseOptions(builder.Configuration)
    .AddNpgsqlDataSource()
    .AddMigrations()
    .AddInfrastructureRepository()
    .AddRedisCaching(builder.Configuration)
    .AddGrpcServices()
    .AddKafka(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    migrationRunner.MigrateUp();
}

app.MapGrpcEndpoints();
app.MapGet("/", () => "ConversionReportService is running. Use gRPC GetReport.");

app.Run();
