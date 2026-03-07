using ConversionReportService.Infrastructure.DataAccess.Database.Migrations;
using ConversionReportService.Infrastructure.DataAccess.Database.Options;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ConversionReportService.Infrastructure.DataAccess.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabaseOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("DatabaseOptions"))
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddNpgsqlDataSource(
        this IServiceCollection services)
    {
        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            DatabaseOptions options =
                sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            var builder = new NpgsqlDataSourceBuilder(
                options.GetConnectionString());

            return builder.Build();
        });

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(r => r
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                    sp.GetRequiredService<IOptions<DatabaseOptions>>()
                        .Value
                        .GetConnectionString())
                .ScanIn(typeof(CreateInitialTables).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}