using OctopusData.Models;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;

namespace OctopusData.Helpers;

public partial class SqLiteHelper
{
    private readonly string _dataFile;
    private Logger _logger;

    public SqLiteHelper(string account, Logger logger)
    {
        _logger = logger;

        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);
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
        SQLiteConnection conn = new SQLiteConnection($"Data Source={_dataFile};Synchronous=Full");
        return conn.OpenAndReturn();
    }

    private void CreateInitialTables()
    {
        string[] statements = ResourceHelper.GetStringResource("SqLite.Initial-Database.sql")
            .Split(Environment.NewLine);

        ExecuteStatements(statements);
    }

    private void ExecuteStatements(string[] statements)
    {
        using SQLiteConnection connection = GetConnection();
        foreach (string statement in statements)
        {
            if (!string.IsNullOrEmpty(statement) && !statement.StartsWith('-'))
            {
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                command.ExecuteNonQuery();
            }
        }
    }

    private bool ColumnExists(string tableName, string columnName)
    {
        bool result = false;

        using SQLiteConnection connection = GetConnection();
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("SELECT sql");
        stringBuilder.AppendLine("FROM sqlite_master");
        stringBuilder.AppendLine($"WHERE type='table' AND name='{tableName}'");

        SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
        SQLiteDataReader? reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                string sql = FieldAsString(reader["sql"]);
                result = sql.Contains(columnName);
            }
        }

        return result;
    }

    private bool ObjectExists(string objectType, string objectName)
    {
        bool result = false;

        using (SQLiteConnection connection = GetConnection())
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT name");
            stringBuilder.AppendLine("FROM sqlite_master");
            stringBuilder.AppendLine($"WHERE type='{objectType}' AND name='{objectName}'");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            SQLiteDataReader? reader = command.ExecuteReader();
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
        string temp = $"{field}";
        return string.IsNullOrEmpty(temp) ? 0 : int.Parse(temp);
    }

    private DateTime FieldAsTime(object field)
    {
        string temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return DateTime.MaxValue;
        }

        return DateTime.ParseExact(temp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private double FieldAsDouble(object field)
    {
        string temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return 0;
        }

        return double.Parse(temp);
    }

    public List<MySummary> GetSummaryInformation()
    {
        List<MySummary> result = new List<MySummary>();

        using (SQLiteConnection connection = GetConnection())
        {
            GetHalfHourlyConsumptionMetric(connection, StringHelper.ProperCase(Constants.Electric));
            GetHalfHourlyConsumptionMetric(connection, StringHelper.ProperCase(Constants.Gas));

            GetHalfHourlyCostsMetric(connection, StringHelper.ProperCase(Constants.Electric));
            GetHalfHourlyCostsMetric(connection, StringHelper.ProperCase(Constants.Gas));

            GetChargeEventsMetric(connection, StringHelper.ProperCase(Constants.Electric));
        }

        return result;

        // Local Functions

        void ExtractMetric(SQLiteDataReader reader, string metric, string fuelType)
        {
            string from = FieldAsString(reader["Min"]);
            string to = FieldAsString(reader["Max"]);
            int count = FieldAsInt(reader["count"]);

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
                MySummary info = new MySummary
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

        void GetHalfHourlyConsumptionMetric(SQLiteConnection connection, string fuelType)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(StartTime) AS Max, MIN(StartTime) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM HalfHourlyConsumption{fuelType}");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            SQLiteDataReader? reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Consumption", fuelType);
                }
            }
        }

        void GetHalfHourlyCostsMetric(SQLiteConnection connection, string fuelType)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(StartTime) AS Max, MIN(StartTime) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM HalfHourlyCosts{fuelType}");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            SQLiteDataReader? reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Costs", fuelType);
                }
            }
        }

        void GetChargeEventsMetric(SQLiteConnection connection, string fuelType)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(StartTime) AS Max, MIN(StartTime) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine("FROM ChargeEvents");

            SQLiteCommand command = new SQLiteCommand(stringBuilder.ToString(), connection);
            SQLiteDataReader? reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Charge Events", fuelType);
                }
            }
        }
    }
}