namespace OctopusData.Models;

public class OctopusHalfHourly
{
    public double Consumption { get; set; }
    public OctopusInterval Interval { get; set; } = new();
}