namespace ConversionReportService.Application.Models.Dtos;

public class CreateReportRequestDto
{
    public long ProductId { get; init; }

    public long CheckoutId { get; init; }

    public DateTime From { get; init; }

    public DateTime To { get; init; }
}