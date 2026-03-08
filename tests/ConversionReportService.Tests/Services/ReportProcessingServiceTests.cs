using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;
using ConversionReportService.Application.ReportServices;
using Moq;
using Xunit;

namespace ConversionReportService.Tests.Services;

public class ReportProcessingServiceTests
{
    [Fact]
    public async Task ProcessAsync_ShouldThrowKeyNotFound_WhenRequestMissing()
    {
        // Arrange
        var repository = new Mock<IReportRepository>();
        repository.Setup(r => r.GetRequestAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportRequest?)null);
        var service = new ReportProcessingService(repository.Object, null!);

        // Act
        Func<Task> act = () =>
            service.ProcessAsync(42, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPropagate_WhenMetricsReadFails()
    {
        // Arrange
        var repository = new Mock<IReportRepository>();
        var request = ReportRequest.FromDatabase(
            id: 42,
            productId: 10,
            checkoutId: 20,
            period: new ReportPeriod(
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow),
            status: ReportStatus.Pending,
            createdAt: DateTime.UtcNow);

        repository.Setup(r => r.GetRequestAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        repository.Setup(r => r.GetMetricsAsync(
                10,
                20,
                request.Period.From,
                request.Period.To,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("metrics unavailable"));
        var service = new ReportProcessingService(repository.Object, null!);

        // Act
        Func<Task> act = () => service.ProcessAsync(42, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
