using System.Text.Json;
using ConversionReportService.Tools.ViewEventPublisher;
using ConversionReportService.Tools.ViewEventPublisher.Settings;

var settings = LoadSettings();
await ViewEventPublisher.RunAsync(settings);

static PublisherSettings LoadSettings()
{
    var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path)) path = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
    if (!File.Exists(path)) return new PublisherSettings();

    var json = File.ReadAllText(path);
    var root = JsonSerializer.Deserialize<PublisherSettingsRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return root?.ViewEventPublisher ?? new PublisherSettings();
}
