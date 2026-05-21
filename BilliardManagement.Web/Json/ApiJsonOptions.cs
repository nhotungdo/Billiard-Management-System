using System.Text.Json;
using System.Text.Json.Serialization;

namespace BilliardManagement.Web.Json
{
    public static class ApiJson
    {
        public static JsonSerializerOptions Options { get; } = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new RoleJsonConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
            }
        };
    }
}
