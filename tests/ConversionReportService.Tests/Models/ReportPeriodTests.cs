using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.ValueObjects;
using Xunit;

namespace ConversionReportService.Tests.Models;

public class ReportPeriodTests
{
    [Fact]
    public void Ctor_ShouldThrowDomainException_WhenFromIsAfterOrEqualToTo()
    {
        // Arrange
        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from;
        Action actEqual = () => new ReportPeriod(from, to);
        Action actAfter = () => new ReportPeriod(from.AddMinutes(1), to);

        // Act
        var equalThrow = Record.Exception(actEqual);
        var afterThrow = Record.Exception(actAfter);

        // Assert
        Assert.IsType<DomainException>(equalThrow);
        Assert.IsType<DomainException>(afterThrow);
    }

    [Fact]
    public void Ctor_ShouldCreate_WhenRangeIsValid()
    {
        // Arrange
        var from = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddHours(1);

        // Act
        var period = new ReportPeriod(from, to);

        // Assert
        Assert.Equal(from, period.From);
        Assert.Equal(to, period.To);
    }
}