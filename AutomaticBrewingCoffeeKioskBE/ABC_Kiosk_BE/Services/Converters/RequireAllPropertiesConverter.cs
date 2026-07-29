using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kiosk.ApiService.Converters
{
    public class RequireAllPropertiesConverter<T> : JsonConverter<T> where T : class, new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                // Parse JSON và kiểm tra các thuộc tính bắt buộc
                using var jsonDoc = JsonDocument.ParseValue(ref reader);
                var jsonProperties = jsonDoc.RootElement.EnumerateObject()
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var missingProperties = typeof(T).GetProperties()
                    .Where(prop => !jsonProperties.Contains(prop.Name))
                    .Select(prop => prop.Name)
                    .ToList();

                if (missingProperties.Any())
                {
                    throw new JsonException($"Missing required properties: {string.Join(", ", missingProperties)}");
                }

                // Tạo một bản sao của options mà không chứa converter để tránh đệ quy
                var newOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
                    ReadCommentHandling = options.ReadCommentHandling,
                    AllowTrailingCommas = options.AllowTrailingCommas,
                    DefaultIgnoreCondition = options.DefaultIgnoreCondition
                };

                var json = jsonDoc.RootElement.GetRawText();
                var result = JsonSerializer.Deserialize<T>(json, newOptions);
                if (result == null)
                {
                    throw new JsonException("Deserialization returned null. Check JSON format or class structure.");
                }

                return result;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error deserializing JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new JsonException("Unexpected error while deserializing JSON", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}