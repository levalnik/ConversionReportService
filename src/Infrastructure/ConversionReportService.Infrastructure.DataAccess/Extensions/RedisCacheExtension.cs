using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Infrastructure.DataAccess.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ConversionReportService.Infrastructure.DataAccess.Extensions;

public static class RedisCacheExtension
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("Redis")["ConnectionString"];

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString!));

        services.AddScoped<IReportCache, RedisReportCache>();

        return services;
    }
}