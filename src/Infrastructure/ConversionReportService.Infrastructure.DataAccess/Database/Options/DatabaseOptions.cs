namespace ConversionReportService.Infrastructure.DataAccess.Database.Options;

public class DatabaseOptions
{
    public string? Host { get; set; }

    public int? Port { get; set; }

    public string? Database { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string GetConnectionString()
        => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
}