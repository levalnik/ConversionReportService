namespace ConversionReportService.Application.Models.Exceptions;

public sealed class UnknownReportStatusException : DomainException
{
    public UnknownReportStatusException(string statusValue)
        : base($"Unknown report status '{statusValue}'.")
    {
    }
}
