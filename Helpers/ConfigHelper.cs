using Microsoft.Extensions.Configuration;

namespace OctopusData.Helpers;

public static class ConfigHelper
{
    public static string GetString(IConfigurationRoot? config, string key, string defaultValue)
    {
        string result = defaultValue;

        if (config != null)
        {
            try
            {
                string? value = config[key];
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return result;
    }

    public static bool GetBoolean(IConfigurationRoot? config, string key, bool defaultValue)
    {
        bool result = defaultValue;

        if (config != null)
        {
            try
            {
                string? value = config[key];
                if (bool.TryParse(value, out result))
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return result;
    }
}