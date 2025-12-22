using System.Collections;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using OctopusData.Models;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    private readonly string _dataFile;
    private Logger _logger;

    public SqLiteHelper(string account, Logger logger)
    {
        _logger = logger;

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _dataFile = Path.Combine(folder, $"{account}.db");

        // Create database if required
        if (!File.Exists(_dataFile))
        {
            SQLiteConnection.CreateFile(_dataFile);
            CreateInitialTables();
        }
    }

    private SQLiteConnection GetConnection()
    {
        var conn = new SQLiteConnection($"Data Source={_dataFile};Synchronous=Full");
        return conn.OpenAndReturn();
    }

    private void CreateInitialTables()
    {
        var statements = ResourceHelper.GetStringResource("SqLite.Initial-Database.sql")
            .Split(Environment.NewLine);

        ExecuteStatements(statements);
    }

    private void ExecuteStatements(string[] statements)
    {
        using var connection = GetConnection();
        foreach (var statement in statements)
        {
            if (!string.IsNullOrEmpty(statement) && !statement.StartsWith('-'))
            {
                var command = new SQLiteCommand(statement, connection);
                command.ExecuteNonQuery();
            }
        }
    }

    private bool ColumnExists(string tableName, string columnName)
    {
        var result = false;

        using var connection = GetConnection();
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("SELECT sql");
        stringBuilder.AppendLine("FROM sqlite_master");
        stringBuilder.AppendLine($"WHERE type='table' AND name='{tableName}'");

        var command = new SQLiteCommand(stringBuilder.ToString(), connection);
        var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var sql = FieldAsString(reader["sql"]);
                result = sql.Contains(columnName);
            }
        }

        return result;
    }

    private bool ObjectExists(string objectType, string objectName)
    {
        var result = false;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT name");
            stringBuilder.AppendLine("FROM sqlite_master");
            stringBuilder.AppendLine($"WHERE type='{objectType}' AND name='{objectName}'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result = true;
                }
            }
        }

        return result;
    }

    private string FieldAsString(object field)
    {
        return $"{field}";
    }

    private int FieldAsInt(object field)
    {
        var temp = $"{field}";
        return string.IsNullOrEmpty(temp) ? 0 : int.Parse(temp);
    }

    private DateTime FieldAsTime(object field)
    {
        var temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return DateTime.MaxValue;
        }

        return DateTime.ParseExact(temp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private double FieldAsDouble(object field)
    {
        var temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return 0;
        }

        return double.Parse(temp);
    }

    public List<MySummary> GetUsageInformation()
    {
        var result = new List<MySummary>();

        using (var connection = GetConnection())
        {
            // Electric first
            GetHalfHourlyUsageMetric(connection, StringHelper.ProperCase(Constants.Electric));
            // Then Gas
            GetHalfHourlyUsageMetric(connection, StringHelper.ProperCase(Constants.Gas));
        }

        return result;

        // Local Functions

        void GetHalfHourlyUsageMetric(SQLiteConnection connection, string fuelType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(StartTime) AS Max, MIN(StartTime) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM HalfHourly{fuelType}");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Half Hourly", fuelType);
                }
            }
        }

        void ExtractMetric(SQLiteDataReader reader, string metric, string fuelType)
        {
            var from = FieldAsString(reader["Min"]);
            var to = FieldAsString(reader["Max"]);
            var count = FieldAsInt(reader["count"]);

            if (from.Length > 16)
            {
                from = from.Substring(0, 16);
            }
            if (to.Length > 16)
            {
                to = to.Substring(0, 16);
            }

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                var info = new MySummary
                {
                    FuelType = StringHelper.ProperCase(fuelType),
                    Metric = metric,
                    From = from,
                    To = to,
                    Records = $"{count:#,##0}"
                };

                result.Add(info);
            }
        }
    }

}