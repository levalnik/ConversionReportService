using ConversionReportService.Application.Contracts.ReportServices;
using ConversionReportService.Application.ReportServices;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionReportService.Application.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportRequestIngestionService, ReportRequestIngestionService>();
        services.AddScoped<IReportProcessingService, ReportProcessingService>();
        services.AddScoped<IEventIngestionService, EventIngestionService>();

        return services;
    }
}
