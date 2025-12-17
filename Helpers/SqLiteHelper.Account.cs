using System.Data.SQLite;
using System.Text;
using OctopusData.Models.Account;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertMeter(Meter meter, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            //stringBuilder.AppendLine("INSERT INTO Meters");
            //stringBuilder.AppendLine("VALUES");
            //stringBuilder.AppendLine($"('{meter.SerialNumber}', '{fuelType}', '{meter.FuelType}', '{meter.Status}')");
            //stringBuilder.AppendLine("ON CONFLICT (SerialNumber)");
            //stringBuilder.AppendLine("DO UPDATE SET SerialNumber = excluded.SerialNumber, FuelType = excluded.FuelType, MeterType = excluded.MeterType, Status = excluded.Status");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterRegisters(Register register, string fuelType)
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