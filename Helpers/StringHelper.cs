using System.Globalization;

namespace OctopusData.Helpers;

public static class StringHelper
{
    public static string ProperCase(string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    public static string ReplaceInvalid(string value)
    {
        return value;
    }
}