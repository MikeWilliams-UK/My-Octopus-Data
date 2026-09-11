using OctopusData.Models;
using System.Data.SQLite;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertProperty(OctopusProperty property)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Properties");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{property.Id}')");
            stringBuilder.AppendLine("ON CONFLICT (Id)");
            stringBuilder.AppendLine("DO UPDATE SET Id = excluded.Id");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterPoints(OctopusMeterPoint meterPoint)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterPoints");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meterPoint.Mpxn}', '{meterPoint.FuelType}', '{meterPoint.ProfileClass}', '{meterPoint.ConsumptionStandard}')");
            stringBuilder.AppendLine("ON CONFLICT (Mpxn)");
            stringBuilder.AppendLine("DO UPDATE SET Mpxn = excluded.Mpxn, FuelType = excluded.FuelType, ProfileClass = excluded.ProfileClass, ConsumptionStandard = excluded.ConsumptionStandard");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeter(OctopusMeter meter)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Meters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meter.SerialNumber}', '{meter.FuelType}')");
            stringBuilder.AppendLine("ON CONFLICT (SerialNumber)");
            stringBuilder.AppendLine("DO UPDATE SET SerialNumber = excluded.SerialNumber, FuelType = excluded.FuelType");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertAgreements(OctopusAgreement agreement)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Agreements");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{agreement.StartDate}', '{agreement.EndDate}', '{agreement.FuelType}', '{agreement.TariffCode}')");
            stringBuilder.AppendLine("ON CONFLICT (StartDate, TariffCode)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("StartDate = excluded.StartDate, EndDate = excluded.EndDate, FuelType = excluded.FuelType, TariffCode = excluded.TariffCode");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterRegisters(OctopusMeterRegister register)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterRegisters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{register.Id}', '{register.Rate}', '{Constants.Electric}')");
            stringBuilder.AppendLine("ON CONFLICT (Id)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("Id = excluded.Id, Rate = excluded.Rate, FuelType = excluded.FuelType");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }
}