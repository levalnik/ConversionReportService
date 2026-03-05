using System.Text.Json;
using StackExchange.Redis;

namespace ConversionReportService.Infrastructure.DataAccess.Caching;

public class RedisReportCache
{
    private readonly IDatabase _database;

    public RedisReportCache(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task SetAsync(long requestId, object value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(
            $"report:{requestId}",
            json,
            ttl);
    }

    public async Task<T?> GetAsync<T>(long requestId, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync($"report:{requestId}");

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }
}