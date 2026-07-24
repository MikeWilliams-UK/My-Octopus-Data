using OctopusData.Models;
using System.Data.SQLite;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public int CountDailyChargeEvents(int year, int month)
    {
        var result = 0;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine("FROM ChargeEvents");
            stringBuilder.AppendLine($"WHERE StartTime LIKE '{year}-{month:D2}%'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            _logger.WriteLine($"  Table ChargeEvents has {result} records like '{year}-{month:D2}%'");
        }

        return result;
    }

    public void UpsertCharger(OctopusCharger charger)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Chargers");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{charger.Id}', '{charger.Name}', '{charger.Status}', '{charger.LastActive}')");
            stringBuilder.AppendLine("ON CONFLICT (Id)");
            stringBuilder.AppendLine("DO UPDATE SET Id = excluded.Id, Name = excluded.Name, Status = excluded.Status, LastActive = excluded.LastActive");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertChargeEvent(OctopusChargeEvent chargeEvent)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            var startTime = DateHelper.SortableTimeAndTime(chargeEvent.StartTime);
            var endTime = DateHelper.SortableTimeAndTime(chargeEvent.EndTime);

            stringBuilder.AppendLine("INSERT INTO ChargeEvents");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{chargeEvent.ChargerId}', '{startTime}', '{endTime}', '{chargeEvent.EnergyAdded}', '{chargeEvent.TypeOfCharge}', '{chargeEvent.Problems}')");
            stringBuilder.AppendLine("ON CONFLICT (ChargerId, StartTime)");
            stringBuilder.AppendLine("DO UPDATE SET ChargerId = excluded.ChargerId, StartTime = excluded.StartTime, EndTime = excluded.EndTime, EnergyAdded = excluded.EnergyAdded, TypeOfCharge = excluded.TypeOfCharge, Problems = excluded.Problems");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }


}