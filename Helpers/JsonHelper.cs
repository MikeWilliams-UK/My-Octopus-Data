using System.Text.Json;

namespace OctopusData.Helpers;

public static class JsonHelper
{
    public static string Prettify(string json)
    {
        var jsonObject = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
    }
}