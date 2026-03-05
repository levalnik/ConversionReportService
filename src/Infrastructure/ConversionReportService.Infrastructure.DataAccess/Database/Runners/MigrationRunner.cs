using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Infrastructure.DataAccess.Database.Runners;

public static class MigrationRunner
{
    public static IServiceCollection AddMigrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(configuration.GetConnectionString("Postgres"))
                .ScanIn(typeof(MigrationRunner).Assembly).For.Migrations());

        return services;
    }
}