using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Infrastructure.DataAccess.Extensions;

public static class InfrastructureRepositoryExtension
{
    public static IServiceCollection AddInfrastructureRepository(this IServiceCollection services)
    {
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
