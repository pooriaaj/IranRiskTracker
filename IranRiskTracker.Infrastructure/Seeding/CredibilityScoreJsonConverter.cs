using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using IranRiskTracker.Domain.ValueObjects;

namespace IranRiskTracker.Infrastructure.Seeding
{
    internal class CredibilityScoreJsonConverter : JsonConverter<CredibilityScore>
    {
        public override CredibilityScore Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Expecting a numeric value between 0.0 and 1.0
            if (reader.TokenType == JsonTokenType.Number)
            {
                var d = reader.GetDecimal();
                return new CredibilityScore(d);
            }

            // Also accept string encoded numbers
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (decimal.TryParse(s, out var d)) return new CredibilityScore(d);
            }

            throw new JsonException($"Unable to convert JSON token to CredibilityScore. TokenType={reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, CredibilityScore value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
