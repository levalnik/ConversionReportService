using Confluent.Kafka;
using ConversionReportService.Infrastructure.Messaging.Contracts;
using Google.Protobuf;
using System.Text.Json;
using ConversionReportService.Tools.KafkaPublisher.Settings;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

var settings = LoadSettings();

var bootstrapServers = RequireString(settings.BootstrapServers, "KafkaPublisher:BootstrapServers");
var topic = RequireString(settings.Topic, "KafkaPublisher:Topic");
var productId = RequireLong(settings.ProductId, "KafkaPublisher:ProductId");
var checkoutId = RequireLong(settings.CheckoutId, "KafkaPublisher:CheckoutId");

var now = DateTime.UtcNow;
var requestId = settings.RequestId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
var from = (settings.From ?? now.AddHours(-1)).ToUniversalTime();
var to = (settings.To ?? now).ToUniversalTime();
var createdAt = (settings.CreatedAt ?? now).ToUniversalTime();

if (from >= to)
    throw new ArgumentException("KafkaPublisher:From must be earlier than KafkaPublisher:To.");

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

static PublisherSettings LoadSettings()
{
    var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path)) path = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
    if (!File.Exists(path)) return new PublisherSettings();

    var json = File.ReadAllText(path);
    var root = JsonSerializer.Deserialize<PublisherSettingsRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return root?.KafkaPublisher ?? new PublisherSettings();
}

static string RequireString(string? value, string key)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException($"Missing required setting '{key}'.");

    return value;
}

static long RequireLong(long? value, string key)
{
    if (!value.HasValue)
        throw new ArgumentException($"Missing required setting '{key}'.");

    return value.Value;
}
