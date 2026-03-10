using ConversionReportService.Application.Abstractions.Repositories;
using Npgsql;

namespace ConversionReportService.Infrastructure.DataAccess.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public EventRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddViewEventAsync(
        long productId,
        long checkoutId,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO product_view_events (product_id, checkout_id, occurred_at)
            VALUES (@productId, @checkoutId, @occurredAt)
            """;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.AddWithValue("checkoutId", checkoutId);
        cmd.Parameters.AddWithValue("occurredAt", occurredAt);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddPaymentEventAsync(
        long productId,
        long checkoutId,
        string status,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO product_payment_events (product_id, checkout_id, status, occurred_at)
            VALUES (@productId, @checkoutId, @status, @occurredAt)
            """;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.AddWithValue("checkoutId", checkoutId);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("occurredAt", occurredAt);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
