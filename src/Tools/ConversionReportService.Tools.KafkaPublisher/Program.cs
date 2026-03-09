using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;
using ConversionReportService.Tools.KafkaPublisher.Settings;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

var settings = LoadSettings();
var argsMap = ToMap(args);

var bootstrapServers = ArgOrSetting("bootstrap-servers", settings.BootstrapServers, required: true);
var topic = ArgOrSetting("topic", settings.Topic, required: true);

var productId = long.Parse(ArgOrSetting("product-id", settings.ProductId?.ToString(), required: true));
var checkoutId = long.Parse(ArgOrSetting("checkout-id", settings.CheckoutId?.ToString(), required: true));

var now = DateTime.UtcNow;
var requestId = long.Parse(ArgOrSetting("request-id", (settings.RequestId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ToString()));
var from = DateTime.Parse(ArgOrSetting("from", (settings.From ?? now.AddHours(-1)).ToString("O"))).ToUniversalTime();
var to = DateTime.Parse(ArgOrSetting("to", (settings.To ?? now).ToString("O"))).ToUniversalTime();
var createdAt = DateTime.Parse(ArgOrSetting("created-at", (settings.CreatedAt ?? now).ToString("O"))).ToUniversalTime();

if (from >= to)
    throw new ArgumentException("'from' must be earlier than 'to'.");

var value = new ReportRequestedValue
{
    RequestId = requestId,
    ProductId = productId,
    CheckoutId = checkoutId,
    From = ProtoTimestamp.FromDateTime(from),
    To = ProtoTimestamp.FromDateTime(to),
    CreatedAt = ProtoTimestamp.FromDateTime(createdAt)
};

var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
using var producer = new ProducerBuilder<long, byte[]>(producerConfig).Build();

var result = await producer.ProduceAsync(topic, new Message<long, byte[]>
{
    Key = requestId,
    Value = value.ToByteArray()
});

Console.WriteLine($"Published: topic={result.Topic}, partition={result.Partition}, offset={result.Offset}, requestId={requestId}");
return;

string ArgOrSetting(string key, string? settingValue, bool required = false)
{
    if (argsMap.TryGetValue(key, out var argValue) && !string.IsNullOrWhiteSpace(argValue))
        return argValue;

    if (!string.IsNullOrWhiteSpace(settingValue))
        return settingValue;

    if (required)
        throw new ArgumentException($"Missing required value: --{key}");

    return string.Empty;
}

static Dictionary<string, string> ToMap(string[] args)
{
    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--")) continue;
        var key = args[i][2..];
        var value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
        map[key] = value;
    }
    return map;
}

static PublisherSettings LoadSettings()
{
    var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path)) path = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
    if (!File.Exists(path)) return new PublisherSettings();

    var json = File.ReadAllText(path);
    var root = JsonSerializer.Deserialize<PublisherSettingsRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return root?.KafkaPublisher ?? new PublisherSettings();
}

