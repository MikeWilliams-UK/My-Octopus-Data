using System.IO;
using System.Reflection;

namespace OctopusData.Helpers;

public static class ResourceHelper
{
    public static string GetStringResource(string resourceName)
    {
        string data = string.Empty;

        Stream? resource = GetBinaryResource(resourceName);
        if (resource != null)
        {
            StreamReader textStreamReader = new StreamReader(resource);
            data = textStreamReader.ReadToEnd();
        }

        return data;
    }

    private static Stream? GetBinaryResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        Stream? data = Stream.Null;

        string fullName = string.Empty;
        int count = 0;

        string[] resources = assembly.GetManifestResourceNames();
        foreach (string s in resources)
        {
            if (s.EndsWith($".{resourceName}"))
            {
                count++;
                fullName = s;
            }
        }

        if (!string.IsNullOrEmpty(fullName))
        {
            data = assembly.GetManifestResourceStream(fullName);
        }

        return count != 1
            ? Stream.Null
            : data;
    }
}