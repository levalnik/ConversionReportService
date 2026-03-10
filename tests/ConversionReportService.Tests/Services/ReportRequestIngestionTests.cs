using ConversionReportService.Application.Abstractions.Repositories;
using ConversionReportService.Application.Models.Events;
using ConversionReportService.Application.ReportServices;
using FluentAssertions;
using Moq;
using Npgsql;
using Xunit;

namespace ConversionReportService.Tests.Services;

public class ReportRequestIngestionTests
{
    private readonly Mock<IReportRepository> _repository = new();
    private readonly Mock<NpgsqlDataSource> _dataSource = new();

    [Fact]
    public async Task IngestAsync_Should_SaveRequest()
    {
        // Arrange
        var evt = new ReportRequestedEvent
        (
            RequestId: 1,
            ProductId: 10,
            CheckoutId: 20,
            From: DateTime.UtcNow.AddDays(-1),
            To: DateTime.UtcNow,
            CreatedAt:  DateTime.UtcNow
        );

        var conn = new Mock<NpgsqlConnection>();
        var tran = new Mock<NpgsqlTransaction>();

        conn.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tran.Object);

        _dataSource.Setup(x => x.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(conn.Object);

        var service = new ReportRequestIngestionService(_repository.Object, _dataSource.Object);

        // Act
        var id = await service.IngestAsync(evt, CancellationToken.None);

        // Assert
        id.Should().Be(1);

        _repository.Verify(x =>
                x.CreateRequestAsync(
                    1,
                    It.IsAny<Application.Models.Requests.ReportRequest>(),
                    conn.Object,
                    tran.Object,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}