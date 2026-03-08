using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Results;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;
using ConversionReportService.Application.ReportServices;
using Moq;
using Xunit;

namespace ConversionReportService.Tests.Services;

public class ReportServiceCachingTests
{
    [Fact]
    public async Task GetReportAsync_ShouldReturnFromCache_WhenPresent()
    {
        // Arrange
        var cached = new ReportResponseDto
        {
            RequestId = 100,
            Status = ReportStatus.Pending.ToString(),
            ConversionRatio = null,
            PaymentsCount = null
        };

        var cache = new Mock<IReportCache>();
        cache.Setup(c => c.GetAsync<ReportResponseDto>(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var repository = new Mock<IReportRepository>(MockBehavior.Strict);
        var service = new ReportService(repository.Object, cache.Object);

        // Act
        var response = await service.GetReportAsync(100, CancellationToken.None);

        // Assert
        Assert.Equal(100, response.RequestId);
        repository.Verify(r => r.GetRequestAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(r => r.GetResultAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReportAsync_ShouldCachePendingWithShortTtl()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = ReportRequest.FromDatabase(
            1,
            11,
            22,
            new ReportPeriod(now.AddHours(-1), now),
            ReportStatus.Pending,
            now);

        var cache = new Mock<IReportCache>();
        cache.Setup(c => c.GetAsync<ReportResponseDto>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResponseDto?)null);
        cache.Setup(c => c.SetAsync(
                1,
                It.IsAny<object>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var repository = new Mock<IReportRepository>();
        repository.Setup(r => r.GetRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        repository.Setup(r => r.GetResultAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResult?)null);

        var service = new ReportService(repository.Object, cache.Object);

        // Act
        await service.GetReportAsync(1, CancellationToken.None);

        // Assert
        cache.Verify(c => c.SetAsync(
                1,
                It.IsAny<object>(),
                It.Is<TimeSpan>(ttl => ttl == TimeSpan.FromSeconds(15)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetReportAsync_ShouldCacheCompletedWithLongTtl()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = ReportRequest.FromDatabase(
            1,
            11,
            22,
            new ReportPeriod(now.AddHours(-1), now),
            ReportStatus.Completed,
            now);
        var result = ReportResult.FromDatabase(1, 30, 0.3, now);

        var cache = new Mock<IReportCache>();
        cache.Setup(c => c.GetAsync<ReportResponseDto>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResponseDto?)null);
        cache.Setup(c => c.SetAsync(
                1,
                It.IsAny<object>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var repository = new Mock<IReportRepository>();
        repository.Setup(r => r.GetRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        repository.Setup(r => r.GetResultAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var service = new ReportService(repository.Object, cache.Object);

        // Act
        var response = await service.GetReportAsync(1, CancellationToken.None);

        // Assert
        cache.Verify(c => c.SetAsync(
                1,
                It.IsAny<object>(),
                It.Is<TimeSpan>(ttl => ttl == TimeSpan.FromMinutes(5)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(0.3, response.ConversionRatio);
        Assert.Equal(30, response.PaymentsCount);
    }

    [Fact]
    public async Task GetReportAsync_ShouldThrow_WhenRequestMissing()
    {
        // Arrange
        var cache = new Mock<IReportCache>();
        cache.Setup(c => c.GetAsync<ReportResponseDto>(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResponseDto?)null);

        var repository = new Mock<IReportRepository>();
        repository.Setup(r => r.GetRequestAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportRequest?)null);

        var service = new ReportService(repository.Object, cache.Object);

        // Act
        Func<Task> act = () => service.GetReportAsync(99, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);
    }
}