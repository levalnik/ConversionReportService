using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Results;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;
using Npgsql;

namespace ConversionReportService.Infrastructure.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ReportRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateRequestAsync(
        ReportRequest request,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO report_requests
            (product_id, checkout_id, period_start, period_end, status, created_at)
            VALUES
            (@productId, @checkoutId, @periodStart, @periodEnd, @status, @createdAt)
            RETURNING id
        """;

        await using var cmd = new NpgsqlCommand(sql, conn, tran);

        cmd.Parameters.AddWithValue("productId", request.ProductId);
        cmd.Parameters.AddWithValue("checkoutId", request.CheckoutId);
        cmd.Parameters.AddWithValue("periodStart", request.Period.From);
        cmd.Parameters.AddWithValue("periodEnd", request.Period.To);
        cmd.Parameters.AddWithValue("status", request.Status.ToString());
        cmd.Parameters.AddWithValue("createdAt", request.CreatedAt);

        var id = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;

        request.SetId(id);

        return id;
    }

    public async Task SaveResultAsync(
        ReportResult result,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO report_results
            (request_id, payments_count, conversion_ratio, generated_at)
            VALUES
            (@requestId, @payments, @ratio, @generatedAt)
        """;

        await using var cmd = new NpgsqlCommand(sql, conn, tran);

        cmd.Parameters.AddWithValue("requestId", result.RequestId);
        cmd.Parameters.AddWithValue("payments", result.PaymentsCount);
        cmd.Parameters.AddWithValue("ratio", result.ConversionRatio);
        cmd.Parameters.AddWithValue("generatedAt", result.GeneratedAt);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        long requestId,
        ReportStatus status,
        NpgsqlConnection conn,
        NpgsqlTransaction tran,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE report_requests
            SET status = @status
            WHERE id = @requestId
        """;

        await using var cmd = new NpgsqlCommand(sql, conn, tran);

        cmd.Parameters.AddWithValue("status", status.ToString());
        cmd.Parameters.AddWithValue("requestId", requestId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<(int ViewsCount, int PaymentsCount)> GetMetricsAsync(
        long productId,
        long checkoutId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*)::int
                 FROM product_view_events
                 WHERE product_id = @productId
                   AND checkout_id = @checkoutId
                   AND occurred_at >= @from
                   AND occurred_at < @to) AS views_count,
                (SELECT COUNT(*)::int
                 FROM product_payment_events
                 WHERE product_id = @productId
                   AND checkout_id = @checkoutId
                   AND status = 'Success'
                   AND occurred_at >= @from
                   AND occurred_at < @to) AS payments_count
        """;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.AddWithValue("checkoutId", checkoutId);
        cmd.Parameters.AddWithValue("from", from);
        cmd.Parameters.AddWithValue("to", to);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return (reader.GetInt32(0), reader.GetInt32(1));
    }

    public async Task<ReportRequest?> GetRequestAsync(
        long id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id,
                   product_id,
                   checkout_id,
                   period_start,
                   period_end,
                   status,
                   created_at
            FROM report_requests
            WHERE id = @id
        """;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var period = new ReportPeriod(
            reader.GetDateTime(3),
            reader.GetDateTime(4)
        );

        var statusValue = reader.GetString(5);
        if (!Enum.TryParse(statusValue, out ReportStatus status))
            throw new InvalidOperationException($"Unknown report status '{statusValue}'.");

        return ReportRequest.FromDatabase(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            period,
            status,
            reader.GetDateTime(6));
    }

    public async Task<ReportResult?> GetResultAsync(
        long requestId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT request_id,
                   payments_count,
                   conversion_ratio,
                   generated_at
            FROM report_results
            WHERE request_id = @requestId
        """;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("requestId", requestId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;
        
        return ReportResult.FromDatabase(
            reader.GetInt64(0),
            reader.GetInt32(1),
            reader.GetDouble(2),
            reader.GetDateTime(3)
        );
    }
}
