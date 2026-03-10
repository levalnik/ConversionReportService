using System.Text.Json;
using ConversionReportService.Tools.KafkaPublisher;
using ConversionReportService.Tools.KafkaPublisher.Settings;

var settings = LoadSettings();
await ReportRequestEventPublisher.RunAsync(settings);
return;

static PublisherSettings LoadSettings()
{
    var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path)) path = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
    if (!File.Exists(path)) return new PublisherSettings();

    var json = File.ReadAllText(path);
    var root = JsonSerializer.Deserialize<PublisherSettingsRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return root?.KafkaPublisher ?? new PublisherSettings();
}
