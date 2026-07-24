namespace OctopusData.Models;

public class OctopusChargeEvent
{
    public string ChargerId { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public double EnergyAdded { get; set; }

    public string TypeOfCharge { get; set; } = string.Empty;
    public string Problems { get; set; } = string.Empty;
}