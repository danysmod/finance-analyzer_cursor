using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Helpers;

public class AnonBank1DateTimeSerializer :  JsonConverter<DateTime>
{
    private const string Format = "dd.MM.yyyy'T'HH:mm:ss";
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (DateTime.TryParseExact(
                value,
                Format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
        {
            return result;
        }
        throw new JsonException($"Invalid DateTime format. Expected: {Format}");
    }
    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}