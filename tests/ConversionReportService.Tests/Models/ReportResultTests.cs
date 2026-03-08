using ConversionReportService.Application.Models.Exceptions;
using ConversionReportService.Application.Models.Results;
using Xunit;

namespace ConversionReportService.Tests.Models;

public class ReportResultTests
{
    [Fact]
    public void Ctor_ShouldThrowDomainException_WhenViewsNegative()
    {
        // Arrange
        Action act = () => new ReportResult(1, -1, 0);

        // Act
        var exception = Record.Exception(act);

        // Assert
        Assert.IsType<DomainException>(exception);
    }

    [Fact]
    public void Ctor_ShouldThrowDomainException_WhenPaymentsNegative()
    {
        // Arrange
        Action act = () => new ReportResult(1, 10, -1);

        // Act
        var exception = Record.Exception(act);

        // Assert
        Assert.IsType<DomainException>(exception);
    }

    [Fact]
    public void Ctor_ShouldCalculateConversionRatio()
    {
        // Arrange
        const int views = 100;
        const int payments = 25;

        // Act
        var result = new ReportResult(1, views, payments);

        // Assert
        Assert.Equal(25, result.PaymentsCount);
        Assert.Equal(0.25, result.ConversionRatio, 5);
    }

    [Fact]
    public void Ctor_ShouldSetZeroRatio_WhenViewsZero()
    {
        // Arrange
        const int views = 0;
        const int payments = 0;

        // Act
        var result = new ReportResult(1, views, payments);

        // Assert
        Assert.Equal(0, result.ConversionRatio);
    }
}