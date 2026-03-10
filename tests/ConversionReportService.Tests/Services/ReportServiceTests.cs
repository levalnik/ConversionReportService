using ConversionReportService.Application.Abstractions.Caching;
using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Models.Dtos;
using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;
using ConversionReportService.Application.ReportServices;
using FluentAssertions;
using Moq;
using Xunit;

namespace ConversionReportService.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _repository = new();
    private readonly Mock<IReportCache> _cache = new();

    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _service = new ReportService(_repository.Object, _cache.Object);
    }

    [Fact]
    public async Task GetReportAsync_Should_ReturnCachedValue()
    {
        // Arrange
        var cached = new ReportResponseDto
        {
            RequestId = 1,
            Status = "Completed",
            ConversionRatio = 0.5,
            PaymentsCount = 10
        };

        _cache.Setup(x => x.GetAsync<ReportResponseDto>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _service.GetReportAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cached);

        _repository.Verify(x => x.GetRequestAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReportAsync_Should_LoadFromRepository_WhenCacheMiss()
    {
        // Arrange
        var request = new ReportRequest(1, 2, new ReportPeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow));

        typeof(ReportRequest)
            .GetProperty("Status")!
            .SetValue(request, ReportStatus.Completed);

        _cache.Setup(x => x.GetAsync<ReportResponseDto>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResponseDto?)null);

        _repository.Setup(x => x.GetRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        _repository.Setup(x => x.GetResultAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Application.Models.Results.ReportResult(1, 100, 10));

        // Act
        var result = await _service.GetReportAsync(1, CancellationToken.None);

        // Assert
        result.RequestId.Should().Be(1);
        result.PaymentsCount.Should().Be(10);

        _cache.Verify(x => x.SetAsync(
            1,
            It.IsAny<object>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetReportAsync_ShouldThrow_WhenRequestNotFound()
    {
        // Arrange
        _cache.Setup(x => x.GetAsync<ReportResponseDto>(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportResponseDto?)null);

        _repository.Setup(x => x.GetRequestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportRequest?)null);

        // Act
        Func<Task> act = async () => await _service.GetReportAsync(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ReportNotFoundException>();
    }
}