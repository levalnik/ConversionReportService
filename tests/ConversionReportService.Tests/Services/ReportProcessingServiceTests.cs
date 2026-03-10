using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.ValueObjects;
using ConversionReportService.Application.ReportServices;
using ConversionReportService.Infrastructure.DataAccess.Database.Migrations;
using ConversionReportService.Infrastructure.DataAccess.Repositories;
using FluentAssertions;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace ConversionReportService.Tests.Services;

public class ReportProcessingServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public ReportProcessingServiceTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        var services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(_postgres.GetConnectionString())
                .ScanIn(typeof(CreateInitialTables).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = services.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }

    [Fact]
    public async Task ProcessAsync_Should_Create_Report_Result()
    {
        // Arrange
        var dataSource = NpgsqlDataSource.Create(_postgres.GetConnectionString());

        var repository = new ReportRepository(dataSource);

        var service = new ReportProcessingService(repository, dataSource);

        var request = new ReportRequest(
            productId: 1,
            checkoutId: 1,
            new ReportPeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow));

        const long externalRequestId = 100;

        await using var conn = await dataSource.OpenConnectionAsync();
        await using var tran = await conn.BeginTransactionAsync();

        var requestId = await repository.CreateRequestAsync(
            externalRequestId,
            request,
            conn,
            tran,
            CancellationToken.None);

        await tran.CommitAsync();

        // Act
        await service.ProcessAsync(requestId, CancellationToken.None);

        // Assert
        var result = await repository.GetResultAsync(requestId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.RequestId.Should().Be(requestId);
    }
}