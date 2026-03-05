namespace ConversionReportService.Application.Models.Dtos;

public class ReportResponseDto
{
    public long RequestId { get; init; }

    public string? Status { get; init; }

    public double? ConversionRatio { get; init; }

    public int? PaymentsCount { get; init; }
}