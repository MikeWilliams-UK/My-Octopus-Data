namespace OctopusData.Models;

public class OctopusMeterRegister
{
    public string Id { get; set; } = string.Empty;
    public string Rate { get; set; } = string.Empty;
    public bool IsSettlement { get; set; }
}