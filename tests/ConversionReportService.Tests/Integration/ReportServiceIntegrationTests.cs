using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.ReportServices;
using ConversionReportService.Infrastructure.DataAccess.Database.Migrations;
using ConversionReportService.Infrastructure.DataAccess.Repositories;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace ConversionReportService.Tests.Integration;

public sealed class ReportServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        _dataSource = NpgsqlDataSource.Create(_db.GetConnectionString());

        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dataSource != null)
            await _dataSource.DisposeAsync();

        await _db.DisposeAsync();
    }

    private async Task ApplyMigrationsAsync()
    {
        var services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(_db.GetConnectionString())
                .ScanIn(typeof(CreateInitialTables).Assembly).For.Migrations())
            .BuildServiceProvider(false);

        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnCalculatedRatioAndPayments()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var from = now.AddHours(-1);
        var to = now;

        const long requestId = 1001;
        const long productId = 11;
        const long checkoutId = 22;

        await SeedRequestAsync(requestId, productId, checkoutId, from, to, now);
        await SeedViewEventsAsync(productId, checkoutId, from.AddMinutes(1), 10);
        await SeedViewEventsAsync(productId, checkoutId, from.AddHours(-2), 2);
        await SeedPaymentEventsAsync(productId, checkoutId, from.AddMinutes(2), 3, "Success");
        await SeedPaymentEventsAsync(productId, checkoutId, from.AddMinutes(3), 2, "Failed");
        await SeedPaymentEventsAsync(productId, checkoutId, from.AddHours(-2), 1, "Success");

        var repository = new ReportRepository(_dataSource!);
        var processing = new ReportProcessingService(repository, _dataSource!);
        var cache = new NoOpReportCache();
        var service = new ReportService(repository, cache);

        // Act
        await processing.ProcessAsync(requestId, CancellationToken.None);
        var response = await service.GetReportAsync(requestId, CancellationToken.None);

        // Assert
        Assert.Equal(ReportStatus.Completed.ToString(), response.Status);
        Assert.Equal(3, response.PaymentsCount);
        Assert.Equal(0.3, response.ConversionRatio ?? 0, 5);
    }

    private async Task SeedRequestAsync(
        long requestId,
        long productId,
        long checkoutId,
        DateTime from,
        DateTime to,
        DateTime createdAt)
    {
        await using var conn = await _dataSource!.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO report_requests
            (id, external_request_id, product_id, checkout_id, period_start, period_end, status, created_at)
            OVERRIDING SYSTEM VALUE
            VALUES (@id, @externalId, @productId, @checkoutId, @from, @to, @status, @createdAt)
            """;
        cmd.Parameters.AddWithValue("id", requestId);
        cmd.Parameters.AddWithValue("externalId", requestId);
        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.AddWithValue("checkoutId", checkoutId);
        cmd.Parameters.AddWithValue("from", from);
        cmd.Parameters.AddWithValue("to", to);
        cmd.Parameters.AddWithValue("status", ReportStatus.Pending.ToString());
        cmd.Parameters.AddWithValue("createdAt", createdAt);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task SeedViewEventsAsync(long productId, long checkoutId, DateTime startAt, int count)
    {
        await using var conn = await _dataSource!.OpenConnectionAsync();
        for (var i = 0; i < count; i++)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO product_view_events (product_id, checkout_id, occurred_at)
                VALUES (@productId, @checkoutId, @occurredAt)
                """;
            cmd.Parameters.AddWithValue("productId", productId);
            cmd.Parameters.AddWithValue("checkoutId", checkoutId);
            cmd.Parameters.AddWithValue("occurredAt", startAt.AddMinutes(i));
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task SeedPaymentEventsAsync(
        long productId,
        long checkoutId,
        DateTime startAt,
        int count,
        string status)
    {
        await using var conn = await _dataSource!.OpenConnectionAsync();
        for (var i = 0; i < count; i++)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO product_payment_events (product_id, checkout_id, status, occurred_at)
                VALUES (@productId, @checkoutId, @status, @occurredAt)
                """;
            cmd.Parameters.AddWithValue("productId", productId);
            cmd.Parameters.AddWithValue("checkoutId", checkoutId);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("occurredAt", startAt.AddMinutes(i));
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private sealed class NoOpReportCache : IReportCache
    {
        public Task SetAsync(long requestId, object value, TimeSpan ttl, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<T?> GetAsync<T>(long requestId, CancellationToken cancellationToken)
            => Task.FromResult(default(T));
    }
}
