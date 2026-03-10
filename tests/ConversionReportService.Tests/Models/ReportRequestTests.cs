using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.Requests;
using ConversionReportService.Application.Models.Statuses;
using ConversionReportService.Application.Models.ValueObjects;
using Xunit;

namespace ConversionReportService.Tests.Models;

public class ReportRequestTests
{
    [Fact]
    public void Ctor_ShouldInitializePendingStatusAndCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var period = new ReportPeriod(before.AddHours(-1), before);

        // Act
        var request = new ReportRequest(11, 22, period);
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(ReportStatus.Pending, request.Status);
        Assert.InRange(request.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void StartProcessing_ShouldMoveStatusToProcessing_WhenPending()
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

        // Act
        request.StartProcessing();

        // Assert
        Assert.Equal(ReportStatus.Processing, request.Status);
    }

    [Fact]
    public void StartProcessing_ShouldThrowDomainException_WhenNotPending()
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

        // Act
        Action act = request.StartProcessing;

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal("Report cannot start processing", ex.Message);
    }

    [Fact]
    public void Complete_ShouldMoveStatusToCompleted_WhenProcessing()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = ReportRequest.FromDatabase(
            1,
            11,
            22,
            new ReportPeriod(now.AddHours(-1), now),
            ReportStatus.Processing,
            now);

        // Act
        request.Complete();

        // Assert
        Assert.Equal(ReportStatus.Completed, request.Status);
    }

    [Fact]
    public void Complete_ShouldThrowDomainException_WhenNotProcessing()
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

        // Act
        Action act = request.Complete;

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal("Report cannot be completed", ex.Message);
    }

    [Fact]
    public void Fail_ShouldMoveStatusToFailed()
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

        // Act
        request.Fail();

        // Assert
        Assert.Equal(ReportStatus.Failed, request.Status);
    }
}
