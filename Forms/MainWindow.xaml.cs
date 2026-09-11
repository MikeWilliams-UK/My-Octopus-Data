using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OctopusData.Helpers;
using OctopusData.Models;
using OctopusData.Models.Account;
using OctopusData.Models.Charging.Devices;
using OctopusData.Models.Charging.Sessions;
using OctopusData.Models.ElectricCost;
using OctopusData.Models.GasCost;
using OctopusData.Models.Usage;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Agreement = OctopusData.Models.Account.Agreement;
using Device = OctopusData.Models.Charging.Devices.Device;
using Edge = OctopusData.Models.Charging.Sessions.Edge;
using ElectricityMeterPoint = OctopusData.Models.Account.ElectricityMeterPoint;
using Property = OctopusData.Models.Account.Property;
using Statistic = OctopusData.Models.GasCost.Statistic;

namespace OctopusData.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfigurationRoot _configuration;

        private bool _cancelRequested;
        private bool _isUpdating;

        private string _stopWhen = string.Empty;

        private HttpHelper? _httpHelper = null;

        private Logger? _logger;
        private int _logNumber;

        private readonly OctopusAccount _account = new();

        private DateTime _supplyDateElectric = DateTime.MaxValue;
        private DateTime _supplyDateGas = DateTime.MaxValue;

        private DateTime _lastDateElectricConsumption = DateTime.MinValue;
        private DateTime _lastDateGasConsumption = DateTime.MinValue;

        private DateTime _lastDateElectricCosts = DateTime.MinValue;
        private DateTime _lastDateGasCosts = DateTime.MinValue;

        private DispatcherTimer? _timer;

        public MainWindow()
        {
            InitializeComponent();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json")
                .Build();
        }

        private void OnLoaded_MainWindow(object sender, RoutedEventArgs e)
        {
            ReadFromRegistry();
        }

        private async void OnClick_LoginAsync(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref _logNumber);

            if (string.IsNullOrEmpty(AccountId.Text) && string.IsNullOrEmpty(ApiKey.Password))
            {
                MessageBox.Show("Account Id and/or Api Key are blank", "Input Error");
            }
            else
            {
                _account.Id = AccountId.Text;

                _httpHelper = new HttpHelper(_configuration, AccountId.Text, ApiKey.Password);
                _httpHelper.SetLogger(_logger);

                SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

                SetMouseCursor();
                WriteToRegistry();

                SetStatusText("Connecting ...");

                Details? details = await _httpHelper.LoginAsync();
                if (details != null)
                {
                    SetStatusText($"Logged in to Account {AccountId.Text}");
                    Login.IsEnabled = false;

                    if (details.Properties.Count == 1)
                    {
                        Property property = details.Properties[0];
                        _account.MovedIn = property.MovedInAt;
                        OctopusProperty octopusProperty = new OctopusProperty
                        {
                            Id = property.Id
                        };
                        sqLiteHelper.UpsertProperty(octopusProperty);

                        // Handle Electricity
                        foreach (ElectricityMeterPoint meterPoint in property.ElectricityMeterPoints)
                        {
                            OctopusMeterPoint octopusMeterPoint = new OctopusMeterPoint
                            {
                                Mpxn = meterPoint.Mpan,
                                FuelType = Constants.Electric,
                                ProfileClass = meterPoint.ProfileClass,
                                ConsumptionStandard = meterPoint.ConsumptionStandard
                            };
                            sqLiteHelper.UpsertMeterPoints(octopusMeterPoint);

                            _account.ElectricMpan = meterPoint.Mpan;
                            foreach (Meter meter in meterPoint.Meters)
                            {
                                OctopusMeter octopusMeter = new OctopusMeter
                                {
                                    SerialNumber = meter.SerialNumber,
                                    FuelType = Constants.Electric
                                };
                                sqLiteHelper.UpsertMeter(octopusMeter);

                                foreach (Register register in meter.Registers)
                                {
                                    OctopusMeterRegister octopusMeterRegister = new OctopusMeterRegister
                                    {
                                        Id = register.Identifier,
                                        Rate = register.Rate,
                                        IsSettlement = register.IsSettlementRegister
                                    };
                                    sqLiteHelper.UpsertMeterRegisters(octopusMeterRegister);
                                }

                                _account.ElectricMeterSerial = meter.SerialNumber;
                            }

                            foreach (Agreement agreement in meterPoint.Agreements)
                            {
                                OctopusAgreement octopusAgreement = new OctopusAgreement
                                {
                                    StartDate = agreement.ValidFrom,
                                    EndDate = agreement.ValidTo,
                                    FuelType = Constants.Electric,
                                    TariffCode = agreement.TariffCode
                                };
                                sqLiteHelper.UpsertAgreements(octopusAgreement);

                                if (agreement.ValidFrom < _supplyDateElectric)
                                {
                                    _supplyDateElectric = agreement.ValidFrom;
                                }
                            }
                        }

                        // Handle Gas
                        foreach (GasMeterPoint meterPoint in property.GasMeterPoints)
                        {
                            OctopusMeterPoint octopusMeterPoint = new OctopusMeterPoint
                            {
                                Mpxn = meterPoint.Mprn,
                                FuelType = Constants.Gas,
                                ConsumptionStandard = meterPoint.ConsumptionStandard
                            };
                            sqLiteHelper.UpsertMeterPoints(octopusMeterPoint);

                            _account.GasMprn = meterPoint.Mprn;

                            foreach (Meter meter in meterPoint.Meters)
                            {
                                OctopusMeter octopusMeter = new OctopusMeter
                                {
                                    SerialNumber = meter.SerialNumber,
                                    FuelType = Constants.Gas
                                };
                                sqLiteHelper.UpsertMeter(octopusMeter);

                                _account.GasMeterSerial = meter.SerialNumber;
                            }

                            foreach (Agreement agreement in meterPoint.Agreements)
                            {
                                OctopusAgreement octopusAgreement = new OctopusAgreement
                                {
                                    StartDate = agreement.ValidFrom,
                                    EndDate = agreement.ValidTo,
                                    FuelType = Constants.Gas,
                                    TariffCode = agreement.TariffCode
                                };
                                sqLiteHelper.UpsertAgreements(octopusAgreement);
                                if (agreement.ValidFrom < _supplyDateGas)
                                {
                                    _supplyDateGas = agreement.ValidFrom;
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Don't know how to handle multiple properties", "Multiple Properties");
                    }
                }

                ClearDown();
                ShowAccountInfo();
            }
        }

        private async void OnClick_GetConsumptionAsync(object sender, RoutedEventArgs e)
        {
            SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                SetStatusText("Fetching Electric Consumption ...");

                DateTime currentDay = DateTime.UtcNow.Date;

                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateElectric && currentDay > _lastDateElectricConsumption)
                {
                    SetStatusText($"Fetching Electric Consumption for {currentDay:yyyy-MM-dd}");

                    Usage? electric = await _httpHelper.ObtainElectricHalfHourlyConsumptionAsync(_account, currentDay);
                    if (electric != null)
                    {
                        Debug.WriteLine($"Retrieved {electric.Results.Count} half-hourly electric records for {currentDay:d}.");

                        if (electric.Results.Count > 0)
                        {
                            if (sqLiteHelper.CountHalfHourly(Constants.Electric,
                                    currentDay.Year, currentDay.Month, currentDay.Day) != 48)
                            {
                                List<OctopusHalfHourlyConsumption> octopusHalfHourlies = [];

                                foreach (Result electricResult in electric.Results)
                                {
                                    OctopusHalfHourlyConsumption octopusHalfHourly = new OctopusHalfHourlyConsumption
                                    {
                                        Consumption = electricResult.Consumption,
                                        Interval = new OctopusInterval
                                        {
                                            Start = electricResult.IntervalStart,
                                            End = electricResult.IntervalEnd
                                        }
                                    };
                                    octopusHalfHourlies.Add(octopusHalfHourly);
                                }

                                Debug.WriteLine($"Saving {electric.Results.Count} half-hourly electric records for {currentDay:d}.");
                                sqLiteHelper.UpsertHalfHourlyConsumption(Constants.Electric, octopusHalfHourlies);
                            }
                        }
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);
                }

                currentDay = DateTime.UtcNow.Date;

                SetStatusText("Fetching Gas Consumption ...");

                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateGas && currentDay > _lastDateGasConsumption)
                {
                    SetStatusText($"Fetching Gas Consumption for {currentDay:yyyy-MM-dd}");

                    Usage? gas = await _httpHelper.ObtainGasHalfHourlyConsumptionAsync(_account, currentDay);
                    if (gas != null)
                    {
                        Debug.WriteLine($"Retrieved {gas.Results.Count} half-hourly gas records for {currentDay:d}.");

                        if (gas.Results.Count > 0)
                        {
                            if (sqLiteHelper.CountHalfHourly(Constants.Gas,
                                    currentDay.Year, currentDay.Month, currentDay.Day) != 48)
                            {
                                List<OctopusHalfHourlyConsumption> octopusHalfHourlies = [];

                                foreach (Result electricResult in gas.Results)
                                {
                                    OctopusHalfHourlyConsumption octopusHalfHourly = new OctopusHalfHourlyConsumption
                                    {
                                        Consumption = electricResult.Consumption,
                                        Interval = new OctopusInterval
                                        {
                                            Start = electricResult.IntervalStart,
                                            End = electricResult.IntervalEnd
                                        }
                                    };
                                    octopusHalfHourlies.Add(octopusHalfHourly);
                                }

                                Debug.WriteLine($"Saving {gas.Results.Count} half-hourly gas records for {currentDay:d}.");
                                sqLiteHelper.UpsertHalfHourlyConsumption(Constants.Gas, octopusHalfHourlies);
                            }
                        }
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);
                }
            }
            catch (Exception exception)
            {
                _logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
                ShowAccountInfo();
            }
        }

        private async void OnClick_GetCostsAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

                SetMouseCursor();
                SetStateOfControls(false);

                DateTime currentDay = DateTime.UtcNow.Date;

                SetStatusText("Fetching Gas Costs ...");

                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateGas && currentDay > _lastDateGasCosts)
                {
                    List<OctopusHalfHourlyCost> costsGas = [];

                    SetStatusText($"Fetching Gas Costs for {currentDay:yyyy-MM-dd}");

                    GasCosts? gasCosts = await _httpHelper.ObtainGasHalfHourlyCostsAsync(_account, currentDay);
                    if (gasCosts != null)
                    {
                        foreach (Models.GasCost.Edge edge in gasCosts.Data.Account.Properties[0].Measurements.Edges)
                        {
                            OctopusHalfHourlyCost costs = new OctopusHalfHourlyCost
                            {
                                Interval = new OctopusInterval
                                {
                                    Start = edge.Node.StartAt,
                                    End = edge.Node.EndAt
                                },
                                Consumption = double.Parse(edge.Node.Value)
                            };

                            foreach (Statistic statistic in edge.Node.MetaData.Statistics)
                            {
                                if (statistic.Type.Equals("STANDING_CHARGE_COST"))
                                {
                                    OctopusCostData cd = new OctopusCostData
                                    {
                                        CostType = "Standing Charge",
                                        CostExcludingVat = double.Parse(statistic.CostExclTax.EstimatedAmount),
                                        CostIncludingVat = double.Parse(statistic.CostInclTax.EstimatedAmount)
                                    };

                                    costs.Costs.Add(cd);
                                }
                                else
                                {
                                    OctopusCostData cd = new OctopusCostData
                                    {
                                        CostType = "Gas",
                                        Consumption = double.Parse(edge.Node.Value),
                                        CostExcludingVat = double.Parse(statistic.CostExclTax.EstimatedAmount),
                                        CostIncludingVat = double.Parse(statistic.CostInclTax.EstimatedAmount)
                                    };

                                    costs.Costs.Add(cd);
                                }
                            }

                            costsGas.Add(costs);
                        }

                        if (costsGas.Count > 0)
                        {
                            sqLiteHelper.UpsertHalfHourlyCosts("Gas", costsGas);
                        }
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);
                }

                currentDay = DateTime.UtcNow.Date;

                SetStatusText("Fetching Electricity Costs ...");

                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateElectric && currentDay > _lastDateElectricCosts)
                {
                    List<OctopusHalfHourlyCost> costsElectric = [];

                    SetStatusText($"Fetching Electricity Costs for {currentDay:yyyy-MM-dd}");

                    ElectricCosts? electricCosts = await _httpHelper.ObtainElectricUsageCostsAsync(_account, currentDay);
                    if (electricCosts != null)
                    {
                        foreach (Models.ElectricCost.Edge edge in electricCosts.Data.Account.Properties[0].Measurements.Edges)
                        {
                            OctopusHalfHourlyCost costs = new OctopusHalfHourlyCost
                            {
                                Interval = new OctopusInterval
                                {
                                    Start = edge.Node.StartAt,
                                    End = edge.Node.EndAt
                                },
                                Consumption = double.Parse(edge.Node.Value)
                            };

                            foreach (Models.ElectricCost.Statistic statistic in edge.Node.MetaData.Statistics)
                            {
                                if (statistic.Type.Equals("STANDING_CHARGE_COST"))
                                {
                                    OctopusCostData cd = new OctopusCostData
                                    {
                                        CostType = "Standing Charge",
                                        RateExcludingVat = double.Parse(statistic.CostInclTax.EstimatedAmount),
                                        CostExcludingVat = double.Parse(statistic.CostExclTax.EstimatedAmount),
                                        RateIncludingVat = double.Parse(statistic.CostExclTax.EstimatedAmount),
                                        CostIncludingVat = double.Parse(statistic.CostInclTax.EstimatedAmount)
                                    };

                                    costs.Costs.Add(cd);
                                }
                                else
                                {
                                    OctopusCostData cd = new OctopusCostData
                                    {
                                        RateExcludingVat = double.Parse(statistic.CostExclTax.PricePerUnit.Amount),
                                        CostExcludingVat = double.Parse(statistic.CostExclTax.EstimatedAmount),
                                        RateIncludingVat = double.Parse(statistic.CostInclTax.PricePerUnit.Amount),
                                        CostIncludingVat = double.Parse(statistic.CostInclTax.EstimatedAmount)
                                    };

                                    if (!string.IsNullOrEmpty(statistic.Value))
                                    {
                                        cd.Consumption = double.Parse(statistic.Value);
                                    }

                                    switch (statistic.Label)
                                    {
                                        case "CONSUMPTION_CHARGE_ECO7_DAY_B":
                                            cd.CostType = "Day";
                                            break;

                                        case "CONSUMPTION_CHARGE_ECO7_NIGHT_B":
                                            cd.CostType = "Night";
                                            break;

                                        case "CONSUMPTION_CHARGE_EV_DEVICE_OFF_PEAK_B":
                                            cd.CostType = "EV Off Peak";
                                            break;

                                        case "CONSUMPTION_CHARGE_EV_DEVICE_PEAK_B":
                                            cd.CostType = "EV Peak";
                                            break;

                                        default:
                                            cd.CostType = statistic.Label;
                                            Debugger.Break();
                                            break;
                                    }

                                    costs.Costs.Add(cd);
                                }
                            }

                            costsElectric.Add(costs);
                        }

                        if (costsElectric.Count > 0)
                        {
                            int upserted = sqLiteHelper.UpsertHalfHourlyCosts("Electric", costsElectric);
                            if (upserted == 0)
                            {
                                // We have exhausted all "new" costs (i.e. date is before 03/08/2026)
                                break;
                            }
                        }
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);
                }
            }
            catch (Exception exception)
            {
                _logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
                ShowAccountInfo();
            }
        }

        private async void OnClick_GetChargingSessionsAsync(object sender, RoutedEventArgs e)
        {
            SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                DateTime today = DateTime.Today;

                SetStatusText($"Fetching Charge History ...");

                DateTime day = new DateTime(today.Year, today.Month, 01, 0, 0, 0, DateTimeKind.Local);

                // ToDo: Add second condition to allow for fetching all time
                while (day > _account.MovedIn)
                {
                    SetStatusText($"Fetching Charge History for {day:yyyy-MM}");

                    List<OctopusCharger> octopusChargers = new List<OctopusCharger>();
                    List<OctopusChargeEvent> octopusChargeEvents = new List<OctopusChargeEvent>();

                    Chargers? chargers = await _httpHelper.ObtainChargersAsync(_account, day, _account.MovedIn);
                    if (chargers != null)
                    {
                        // Extract data from API
                        foreach (Device device in chargers.Data.Devices)
                        {
                            OctopusCharger charger = new OctopusCharger
                            {
                                Id = device.Id,
                                Name = device.Name
                            };

                            if (device.PublicSession != null && device.PublicSession.Edges.Any())
                            {
                                charger.LastActive = device.PublicSession.Edges[0].Cursor;
                            }
                            if (device.BoostSession != null && device.BoostSession.Edges.Any())
                            {
                                charger.LastActive = device.BoostSession.Edges[0].Cursor;
                            }
                            if (device.SmartSession != null && device.SmartSession.Edges.Any())
                            {
                                charger.LastActive = device.SmartSession.Edges[0].Cursor;
                            }

                            charger.Status = device.Status.Current;

                            octopusChargers.Add(charger);
                        }

                        foreach (OctopusCharger charger in octopusChargers)
                        {
                            ChargeHistory chargeHistory = await _httpHelper.ObtainChargeHistoryAsync(_account, day, charger.Id);

                            if (chargeHistory != null && chargeHistory.Data.Devices.Any())
                            {
                                if (chargeHistory.Data.Devices[0].ChargingSessions != null)
                                {
                                    foreach (Edge edge in chargeHistory.Data.Devices[0].ChargingSessions.Edges)
                                    {
                                        OctopusChargeEvent octopusChargeEvent = new OctopusChargeEvent
                                        {
                                            ChargerId = charger.Id,
                                            StartTime = edge.Node.Start,
                                            EndTime = edge.Node.End,
                                            EnergyAdded = double.Parse(edge.Node.EnergyAdded.Value),
                                            TypeOfCharge = edge.Node.Type
                                        };

                                        if (edge.Node.Problems != null && edge.Node.Problems.Any())
                                        {
                                            StringBuilder stringBuilder = new StringBuilder();
                                            foreach (Problem problem in edge.Node.Problems)
                                            {
                                                if (!string.IsNullOrEmpty(problem.Cause))
                                                {
                                                    stringBuilder.AppendLine(problem.Cause);
                                                }
                                                if (!string.IsNullOrEmpty(problem.TruncationCause))
                                                {
                                                    stringBuilder.AppendLine(problem.TruncationCause);
                                                }
                                            }
                                            octopusChargeEvent.Problems = stringBuilder.ToString().Trim();
                                        }

                                        octopusChargeEvents.Add(octopusChargeEvent);
                                    }
                                }
                            }
                        }

                        // Write the data to SQLite
                        foreach (OctopusCharger charger in octopusChargers)
                        {
                            sqLiteHelper.UpsertCharger(charger);
                        }

                        foreach (OctopusChargeEvent chargeEvent in octopusChargeEvents)
                        {
                            sqLiteHelper.UpsertChargeEvent(chargeEvent);
                        }

                        day = day.AddMonths(-1);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
                ShowAccountInfo();
            }
        }

        private void OnTextChanged_VisibleApiKey(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }
            _isUpdating = true;
            ApiKey.Password = VisibleApiKey.Text;
            _isUpdating = false;
        }

        private void OnPasswordChanged_ApiKey(object sender, RoutedEventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }
            _isUpdating = true;
            VisibleApiKey.Text = ApiKey.Password;
            _isUpdating = false;
        }

        private void OnChecked_Reveal(object sender, RoutedEventArgs e)
        {
            VisibleApiKey.Visibility = Visibility.Visible;
            ApiKey.Visibility = Visibility.Collapsed;
        }

        private void OnUnchecked_Reveal(object sender, RoutedEventArgs e)
        {
            VisibleApiKey.Visibility = Visibility.Collapsed;
            ApiKey.Visibility = Visibility.Visible;
        }

        private void OnSelectionChanged_StopWhen(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OnClick_ExportUsage(object sender, RoutedEventArgs e)
        {
        }

        private void OnClick_CancelOperations(object sender, RoutedEventArgs e)
        {
        }

        private void ShowAccountInfo()
        {
            SetStatusText($"Account Id: {_account.Id}");
            SqLiteHelper sqlite = new SqLiteHelper(_account.Id, _logger!);

            List<MySummary> summary = sqlite.GetSummaryInformation();

            MySummary? electricConsumption = summary.FirstOrDefault(s => s is { FuelType: Constants.Electric, Metric: "Consumption" });
            if (electricConsumption != null)
            {
                _lastDateElectricConsumption = DateTime.ParseExact(electricConsumption.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            MySummary? gasConsumption = summary.FirstOrDefault(s => s is { FuelType: Constants.Gas, Metric: "Consumption" });
            if (gasConsumption != null)
            {
                _lastDateGasConsumption = DateTime.ParseExact(gasConsumption.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            MySummary? electricCosts = summary.FirstOrDefault(s => s is { FuelType: Constants.Electric, Metric: "Costs" });
            if (electricCosts != null)
            {
                _lastDateElectricCosts = DateTime.ParseExact(electricCosts.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            MySummary? gasCosts = summary.FirstOrDefault(s => s is { FuelType: Constants.Gas, Metric: "Costs" });
            if (gasCosts != null)
            {
                _lastDateGasCosts = DateTime.ParseExact(gasCosts.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            AccountStatistics.ItemsSource = summary;
        }

        public void SetStatusText(string message, bool log = false)
        {
            if (log)
            {
                _logger?.WriteLine(message);
            }
            Status.Text = message;
            DoWpfEvents();
        }

        private static void DoWpfEvents()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            }
            catch
            {
                // Nothing we can do here
            }
        }

        private void ClearDown()
        {
            CursorManager.ClearWaitCursor(CancelOperations);
            _cancelRequested = false;

            SetStateOfControls(true);

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void SetMouseCursor()
        {
            CursorManager.SetWaitCursorExcept(CancelOperations);
        }

        private void SetStateOfControls(bool state)
        {
            StopWhen.IsEnabled = state;
            ReadConsumption.IsEnabled = state;
            ReadCosts.IsEnabled = state;
            ReadChargingSessions.IsEnabled = state;
            ExportUsage.IsEnabled = state;
            CancelOperations.IsEnabled = !state;
        }

        private void ReadFromRegistry()
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@$"SOFTWARE\{Constants.ApplicationName}");
            if (key != null)
            {
                AccountId.Text = key.GetValue("AccountId")?.ToString();
                ApiKey.Password = key.GetValue("Api-Key")?.ToString();
            }
        }

        private void WriteToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@$"SOFTWARE\{Constants.ApplicationName}");

            key.SetValue("AccountId", AccountId.Text);
            key.SetValue("Api-Key", ApiKey.Password);
        }
    }
}