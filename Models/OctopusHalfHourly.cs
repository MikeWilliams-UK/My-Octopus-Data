namespace OctopusData.Models;

public class OctopusHalfHourly
{
    public OctopusInterval Interval { get; set; } = new();

    public double Consumption { get; set; }
}