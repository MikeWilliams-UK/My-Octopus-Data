using OctopusData.Models;
using System.Data.SQLite;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public int UpsertHalfHourlyCosts(string fuelType, List<OctopusHalfHourlyCost> items)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            int upserted = 0;

            SQLiteTransaction? transaction = connection.BeginTransaction();

            foreach (OctopusHalfHourlyCost item in items)
            {
                foreach (OctopusCostData cost in item.Costs)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    string timeStamp = DateHelper.SortableTimeAndTime(item.Interval.Start);

                    stringBuilder.AppendLine($"INSERT INTO HalfHourlyCosts{fuelType}");
                    stringBuilder.AppendLine("VALUES");
                    stringBuilder.Append($"('{timeStamp}', {cost.Consumption}, '{cost.CostType}',");
                    stringBuilder.Append($"'{cost.RateExcludingVat}', '{cost.CostExcludingVat}',");
                    stringBuilder.Append($"'{cost.RateIncludingVat}', '{cost.CostIncludingVat}'");
                    stringBuilder.AppendLine(")");
                    stringBuilder.AppendLine("ON CONFLICT (StartTime, CostType)");
                    stringBuilder.AppendLine("DO UPDATE SET");
                    stringBuilder.AppendLine("Consumption = excluded.Consumption, RateExcVat = excluded.RateExcVat, CostExcVat = excluded.CostExcVat, RateIncVat = excluded.RateIncVat, CostIncVat = excluded.CostIncVat");

                    SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
                    upserted += command.ExecuteNonQuery();
                }
            }

            transaction.Commit();

            return upserted;
        }
    }
}