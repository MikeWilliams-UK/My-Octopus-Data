using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace OctopusData.Helpers;

public class Logger
{
    private readonly string _suffix;

    public Logger(ref int logNumber)
    {
        logNumber++;

        _suffix = $"{Environment.ProcessId:X6}-{logNumber:000}";
    }

    public void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            using (StreamWriter streamWriter = File.AppendText(GetFileName()))
            {
                streamWriter.WriteLine($"{DateHelper.LogEntryTimestamp()} - {message}");
                Debug.WriteLine(message);
            }
        }
    }

    private string GetFileName()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Logs")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Logs"));
        }

        string fileName = Path.Combine(folder, "Logs", $"{DateHelper.LogFileSuffix(_suffix)}.log");

        return fileName;
    }

    private string JsonPrettify(string json)
    {
        using JsonDocument jDoc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public void DumpJson(string responseType, string json)
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Dump")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Dump"));
        }

        string fileName = Path.Combine(folder, "Dump", $"{DateHelper.LogFileSuffix()} {responseType}.json");

        File.WriteAllText(fileName, JsonPrettify(json));
    }
}