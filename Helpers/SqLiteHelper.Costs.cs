using OctopusData.Models;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertHalfHourlyCosts(string fuelType, List<OctopusHalfHourlyCost> items)
    {
        using (var connection = GetConnection())
        {
            var transaction = connection.BeginTransaction();

            foreach (var item in items)
            {
                foreach (var cost in item.Costs)
                {
                    var stringBuilder = new StringBuilder();

                    var timeStamp = DateHelper.SortableTimeAndTime(item.Interval.Start);

                    stringBuilder.AppendLine($"INSERT INTO HalfHourlyCosts{fuelType}");
                    stringBuilder.AppendLine("VALUES");
                    stringBuilder.Append($"('{timeStamp}', {cost.Consumption}, '{cost.CostType}',");
                    stringBuilder.Append($"'{cost.RateExcludingVat}', '{cost.CostExcludingVat}',");
                    stringBuilder.Append($"'{cost.RateIncludingVat}', '{cost.CostIncludingVat}'");
                    stringBuilder.AppendLine(")");
                    stringBuilder.AppendLine("ON CONFLICT (StartTime, CostType)");
                    stringBuilder.AppendLine("DO UPDATE SET");
                    stringBuilder.AppendLine("Consumption = excluded.Consumption, RateExcVat = excluded.RateExcVat, CostExcVat = excluded.CostExcVat, RateIncVat = excluded.RateIncVat, CostIncVat = excluded.CostIncVat");

                    Debug.WriteLine(stringBuilder.ToString());
                    var command = new SQLiteCommand(stringBuilder.ToString(), connection);
                    command.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }
    }
}