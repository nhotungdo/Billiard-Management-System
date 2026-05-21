using System.Text.Json;
using System.Text.Json.Serialization;

namespace BilliardManagement.Web.Json
{
    /// <summary>
    /// Deserializes role from API as int (1, 2) or string ("Admin", "Staff").
    /// </summary>
    public class RoleJsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return 0;
                if (int.TryParse(s, out var n)) return n;
                if (s.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return 1;
                if (s.Equals("Staff", StringComparison.OrdinalIgnoreCase)) return 2;
            }

            throw new JsonException($"Cannot convert role value to int.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);
    }
}
