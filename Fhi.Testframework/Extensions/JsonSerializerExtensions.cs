using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fhi.TestFramework.Extensions
{
    internal static class JsonSerializerExtensions
    {
        public static JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public static string Serialize(this object obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }

        public static T? Deserialize<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
