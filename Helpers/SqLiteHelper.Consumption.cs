using OctopusData.Models;
using System.Data.SQLite;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public int CountHalfHourly(string fuelTYpe, int year, int month, int day)
    {
        int result = 0;

        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine($"FROM HalfHourlyConsumption{fuelTYpe}");
            stringBuilder.AppendLine($"WHERE StartTime LIKE '{year}-{month:D2}-{day:D2}%'");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            _logger.WriteLine($"  Table Daily{fuelTYpe} has {result} records like '{year}-{month:D2}-{day:D2}%'");
        }

        return result;
    }

    public void UpsertHalfHourlyConsumption(string fuelType, List<OctopusHalfHourlyConsumption> items)
    {
        using (SQLiteConnection connection = GetConnection())
        {
            SQLiteTransaction? transaction = connection.BeginTransaction();

            foreach (OctopusHalfHourlyConsumption item in items)
            {
                StringBuilder stringBuilder = new StringBuilder();

                string timeStamp = DateHelper.SortableTimeAndTime(item.Interval.Start);

                stringBuilder.AppendLine($"INSERT INTO HalfHourlyConsumption{fuelType}");
                stringBuilder.AppendLine("VALUES");
                stringBuilder.AppendLine($"('{timeStamp}', {item.Consumption})");
                stringBuilder.AppendLine("ON CONFLICT (StartTime)");
                stringBuilder.AppendLine("DO UPDATE SET Consumption = excluded.Consumption");

                SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public List<OctopusHalfHourlyConsumption> FetchHalfHourly(string fuelType)
    {
        List<OctopusHalfHourlyConsumption> result = new List<OctopusHalfHourlyConsumption>();

        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT StartTime, Consumption");
            stringBuilder.AppendLine($"FROM HalfHourlyConsumption{fuelType}");
            stringBuilder.AppendLine("ORDER BY StartTime DESC");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (SQLiteDataReader? reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        OctopusHalfHourlyConsumption dto = new OctopusHalfHourlyConsumption
                        {
                            Consumption = FieldAsDouble(reader["Consumption"]),
                            Interval = new OctopusInterval { Start = FieldAsTime(reader["StartTime"]) }
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }
}