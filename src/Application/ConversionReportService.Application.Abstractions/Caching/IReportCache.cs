namespace ConversionReportService.Application.Abstractions.Caching;

public interface IReportCache
{
    Task SetAsync(long requestId, object value, TimeSpan ttl, CancellationToken cancellationToken);

    Task<T?> GetAsync<T>(long requestId, CancellationToken cancellationToken);
}