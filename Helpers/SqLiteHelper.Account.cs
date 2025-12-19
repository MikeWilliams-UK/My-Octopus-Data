using System.Data.SQLite;
using System.Text;
using OctopusData.Models;
using OctopusData.Models.Account;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertProperty(OctopusProperty property)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Properties");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{property.Id}')");
            stringBuilder.AppendLine("ON CONFLICT (Id)");
            stringBuilder.AppendLine("DO UPDATE SET Id = excluded.Id");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterPoints(OctopusMeterPoint meterPoint)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterPoints");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meterPoint.Mpxn}', '{meterPoint.FuelType}', '{meterPoint.ProfileClass}', '{meterPoint.ConsumptionStandard}')");
            stringBuilder.AppendLine("ON CONFLICT (Mpxn)");
            stringBuilder.AppendLine("DO UPDATE SET Mpxn = excluded.Mpxn, FuelType = excluded.FuelType, ProfileClass = excluded.ProfileClass, ConsumptionStandard = excluded.ConsumptionStandard");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeter(OctopusMeter meter)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Meters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meter.SerialNumber}', '{meter.FuelType}')");
            stringBuilder.AppendLine("ON CONFLICT (SerialNumber)");
            stringBuilder.AppendLine("DO UPDATE SET SerialNumber = excluded.SerialNumber, FuelType = excluded.FuelType");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertAgreements(OctopusAgreement agreement)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Agreements");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{agreement.StartDate}', '{agreement.EndDate}', '{agreement.FuelType}', '{agreement.TariffCode}')");
            stringBuilder.AppendLine("ON CONFLICT (StartDate, TariffCode)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("  StartDate = excluded.StartDate, EndDate = excluded.EndDate, FuelType = excluded.FuelType, TariffCode = excluded.TariffCode");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterRegisters(Register register)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            //stringBuilder.AppendLine("INSERT INTO MeterRegisters");
            //stringBuilder.AppendLine("VALUES");
            //stringBuilder.AppendLine($"('{register.StartDate}', '{register.EndDate}', '{fuelType}',");
            //stringBuilder.AppendLine($" '{register.MeterSerialNumber}', '{register.Id}',");
            //stringBuilder.AppendLine($" '{register.TimingCategory}', '{register.UnitOfMeasurement}')");
            //stringBuilder.AppendLine("ON CONFLICT (StartDate, MeterSerialNumber, Id)");
            //stringBuilder.AppendLine("DO UPDATE SET");
            //stringBuilder.AppendLine("  StartDate = excluded.StartDate, EndDate = excluded.EndDate, FuelType = excluded.FuelType,");
            //stringBuilder.AppendLine("  Id = excluded.Id, TimingCategory = excluded.TimingCategory, UnitOfMeasurement = excluded.UnitOfMeasurement,");
            //stringBuilder.AppendLine("  MeterSerialNumber = excluded.MeterSerialNumber");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }
}