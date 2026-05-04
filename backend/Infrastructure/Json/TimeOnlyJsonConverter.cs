using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnoopyAirlines.Infrastructure.Json
{
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private static readonly string[] SupportedFormats =
        [
            "HH:mm",
            "HH:mm:ss",
            "HH:mm:ss.FFFFFFF"
        ];

        public override TimeOnly Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (   !string.IsNullOrWhiteSpace(value)
                && TimeOnly.TryParseExact(
                    value.Trim(),
                    SupportedFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var time))
            {
                return time;
            }

            throw new JsonException("Time must be formatted as HH:mm.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeOnly value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HH:mm", CultureInfo.InvariantCulture));
        }
    }
}
