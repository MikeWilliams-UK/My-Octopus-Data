using System.Data.SQLite;
using System.Text;
using OctopusData.Models;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    public int CountHalfHourly(string fuelTYpe, int year, int month, int day)
    {
        var result = 0;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine($"FROM HalfHourly{fuelTYpe}");
            stringBuilder.AppendLine($"WHERE StartTime LIKE '{year}-{month:D2}-{day:D2}%'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            _logger.WriteLine($"  Table Daily{fuelTYpe} has {result} records like '{year}-{month:D2}-{day:D2}%'");
        }

        return result;
    }

    public void UpsertHalfHourly(string fuelType, List<OctopusHalfHourly> items)
    {
        using (var connection = GetConnection())
        {
            var transaction = connection.BeginTransaction();

            foreach (var item in items)
            {
                var stringBuilder = new StringBuilder();

                var timeStamp = item.Interval.Start.ToString("yyyy-MM-dd HH:mm:ss");

                stringBuilder.AppendLine($"INSERT INTO HalfHourly{fuelType}");
                stringBuilder.AppendLine("VALUES");
                stringBuilder.AppendLine($"('{timeStamp}', {item.Consumption})");
                stringBuilder.AppendLine("ON CONFLICT (StartTime)");
                stringBuilder.AppendLine("DO UPDATE SET Consumption = excluded.Consumption");

                var command = new SQLiteCommand(stringBuilder.ToString(), connection);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public List<OctopusHalfHourly> FetchHalfHourly(string fuelType)
    {
        var result = new List<OctopusHalfHourly>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT StartTime, Consumption");
            stringBuilder.AppendLine($"FROM HalfHourly{fuelType}");
            stringBuilder.AppendLine("ORDER BY StartTime DESC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new OctopusHalfHourly
                        {
                            Consumption = FieldAsDouble(reader["Consumption"]),
                            Interval = new OctopusInterval(){Start = FieldAsTime(reader["StartTime"]) }
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }
}