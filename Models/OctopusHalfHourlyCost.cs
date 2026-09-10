namespace OctopusData.Models;

public class OctopusHalfHourlyCost
{
    public OctopusInterval Interval { get; set; } = new();

    public double Consumption { get; set; }

    public List<OctopusCostData> Costs { get; set; } = [];

    public override string ToString()
    {
        return $"{Interval.Start:d} {Interval.Start:HH:mm}-{Interval.End:HH:mm} {Consumption:N}kWh";
    }
}

public class OctopusCostData
{
    public string CostType { get; set; } = string.Empty;

    public double Consumption { get; set; }

    public double RateIncludingVat { get; set; }
    public double CostIncludingVat { get; set; }

    public double RateExcludingVat { get; set; }
    public double CostExcludingVat { get; set; }

    public override string ToString()
    {
        return CostType.Equals("Standing Charge")
            ? $"'{CostType}' {CostIncludingVat:N}p"
            : $"{Consumption:N}kWh '{CostType}' {CostIncludingVat:N}p";
    }
}