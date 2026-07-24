namespace OctopusData.Models;

public class OctopusCharger
{
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime LastActive { get; set; }
}