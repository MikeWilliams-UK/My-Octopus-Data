using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OctopusData.Helpers;
using OctopusData.Models;
using OctopusData.Models.Account;
using OctopusData.Models.Charging.Devices;
using OctopusData.Models.Charging.Sessions;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Device = OctopusData.Models.Charging.Devices.Device;
using Edge = OctopusData.Models.Charging.Sessions.Edge;

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

        private HttpHelper _httpHelper;

        private Logger? _logger;
        private int logNumber;

        private OctopusAccount _account = new OctopusAccount();

        private DateTime _supplyDateElectric = DateTime.MaxValue;
        private DateTime _supplyDateGas = DateTime.MaxValue;

        private DateTime _lastDateElectric = DateTime.MinValue;
        private DateTime _lastDateGas = DateTime.MinValue;

        private DispatcherTimer _timer;

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

        private async void OnClick_Login(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);

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

                var details = await _httpHelper.LoginAsync();
                if (details != null)
                {
                    SetStatusText($"Logged in to Account {AccountId.Text}");
                    Login.IsEnabled = false;

                    if (details.Properties.Count == 1)
                    {
                        var property = details.Properties[0];
                        _account.MovedIn = property.MovedInAt;
                        var octopusProperty = new OctopusProperty
                        {
                            Id = property.Id
                        };
                        sqLiteHelper.UpsertProperty(octopusProperty);

                        // Handle Electricity
                        foreach (var meterPoint in property.ElectricityMeterPoints)
                        {
                            var octopusMeterPoint = new OctopusMeterPoint
                            {
                                Mpxn = meterPoint.Mpan,
                                FuelType = Constants.Electric,
                                ProfileClass = meterPoint.ProfileClass,
                                ConsumptionStandard = meterPoint.ConsumptionStandard
                            };
                            sqLiteHelper.UpsertMeterPoints(octopusMeterPoint);

                            _account.ElectricMpan = meterPoint.Mpan;
                            foreach (var meter in meterPoint.Meters)
                            {
                                var octopusMeter = new OctopusMeter
                                {
                                    SerialNumber = meter.SerialNumber,
                                    FuelType = Constants.Electric
                                };
                                sqLiteHelper.UpsertMeter(octopusMeter);

                                foreach (Register register in meter.Registers)
                                {
                                    var octopusMeterRegister = new OctopusMeterRegister
                                    {
                                        Id = register.Identifier,
                                        Rate = register.Rate,
                                        IsSettlement = register.IsSettlementRegister
                                    };
                                    sqLiteHelper.UpsertMeterRegisters(octopusMeterRegister);
                                }

                                _account.ElectricMeterSerial = meter.SerialNumber;
                            }

                            foreach (var agreement in meterPoint.Agreements)
                            {
                                var octopusAgreement = new OctopusAgreement
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
                        foreach (var meterPoint in property.GasMeterPoints)
                        {
                            var octopusMeterPoint = new OctopusMeterPoint
                            {
                                Mpxn = meterPoint.Mprn,
                                FuelType = Constants.Gas,
                                ConsumptionStandard = meterPoint.ConsumptionStandard
                            };
                            sqLiteHelper.UpsertMeterPoints(octopusMeterPoint);

                            _account.GasMprn = meterPoint.Mprn;

                            foreach (var meter in meterPoint.Meters)
                            {
                                var octopusMeter = new OctopusMeter
                                {
                                    SerialNumber = meter.SerialNumber,
                                    FuelType = Constants.Gas
                                };
                                sqLiteHelper.UpsertMeter(octopusMeter);

                                _account.GasMeterSerial = meter.SerialNumber;
                            }

                            foreach (var agreement in meterPoint.Agreements)
                            {
                                var octopusAgreement = new OctopusAgreement
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

        private async void OnClick_Costs(object sender, RoutedEventArgs e)
        {
            var day = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Local);

            var electric = await _httpHelper.ObtainElectricHalfHourlyCostsAsync(_account, day);
            Debug.WriteLine(electric.Data.Account.Properties[0].Measurements.Edges.Count);
            var gas = await _httpHelper.ObtainGasHalfHourlyCostsAsync(_account, day);
            Debug.WriteLine(gas.Data.Account.Properties[0].Measurements.Edges.Count);

            // ToDo: Save these to the database

            return;
        }

        private async void OnClick_ChargingSessions(object sender, RoutedEventArgs e)
        {
            SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                var today = DateTime.Today;

                var day = new DateTime(today.Year, today.Month, 01, 0, 0, 0, DateTimeKind.Local);

                while (day > _account.MovedIn)
                {
                    SetStatusText($"Fetching Charge History for {day:yyyy-MM}");

                    Chargers chargers = await _httpHelper.ObtainChargersAsync(_account, day, _account.MovedIn);
                    List<OctopusCharger> octopusChargers = new List<OctopusCharger>();
                    foreach (Device device in chargers.Data.Devices)
                    {
                        OctopusCharger charger = new OctopusCharger
                        {
                            Id = device.Id,
                            Name = device.Name
                        };

                        if (device.PublicSession.Edges.Any())
                        {
                            charger.LastActive = device.PublicSession.Edges[0].Cursor;
                        }
                        if (device.BoostSession.Edges.Any())
                        {
                            charger.LastActive = device.BoostSession.Edges[0].Cursor;
                        }
                        if (device.SmartSession.Edges.Any())
                        {
                            charger.LastActive = device.SmartSession.Edges[0].Cursor;
                        }

                        charger.Status = device.Status.Current;

                        octopusChargers.Add(charger);
                    }

                    List<OctopusChargeEvent> octopusChargeEvents = new List<OctopusChargeEvent>();

                    foreach (OctopusCharger charger in octopusChargers)
                    {
                        ChargeHistrory chargeHistory = await _httpHelper.ObtainChargeHistoryAsync(_account, day, charger.Id);

                        if (chargeHistory != null && chargeHistory.Data.Devices.Any())
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

        private async void OnClick_ReadUsageAsync(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);

            SqLiteHelper sqLiteHelper = new SqLiteHelper(_account.Id, _logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                // Fetch Electric Usage
                SetStatusText("Fetching Electric usage ...");

                var currentDay = DateTime.UtcNow.Date;
                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateElectric && currentDay > _lastDateElectric)
                {
                    var electric = await _httpHelper.ObtainElectricHalfHourlyUsageAsync(_account, currentDay);
                    if (electric != null)
                    {
                        Debug.WriteLine($"Retrieved {electric.Results.Count} half-hourly electric records for {currentDay:d}.");

                        if (electric.Results.Count > 0)
                        {
                            if (sqLiteHelper.CountHalfHourly(Constants.Electric,
                                    currentDay.Year, currentDay.Month, currentDay.Day) != 48)
                            {
                                List<OctopusHalfHourly> octopusHalfHourlies = [];

                                foreach (var electricResult in electric.Results)
                                {
                                    var octopusHalfHourly = new OctopusHalfHourly
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
                                sqLiteHelper.UpsertHalfHourly(Constants.Electric, octopusHalfHourlies);
                            }
                        }
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);
                }

                currentDay = DateTime.UtcNow.Date;

                // Fetch Gas usage
                SetStatusText("Fetching Gas usage ...");

                // ToDo: Change second condition to allow for fetching all time
                while (currentDay > _supplyDateGas && currentDay > _lastDateGas)
                {
                    var gas = await _httpHelper.ObtainGasHalfHourlyUsageAsync(_account, currentDay);
                    if (gas != null)
                    {
                        Debug.WriteLine($"Retrieved {gas.Results.Count} half-hourly gas records for {currentDay:d}.");

                        if (gas.Results.Count > 0)
                        {
                            if (sqLiteHelper.CountHalfHourly(Constants.Gas,
                                    currentDay.Year, currentDay.Month, currentDay.Day) != 48)
                            {
                                List<OctopusHalfHourly> octopusHalfHourlies = [];

                                foreach (var electricResult in gas.Results)
                                {
                                    var octopusHalfHourly = new OctopusHalfHourly
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
                                sqLiteHelper.UpsertHalfHourly(Constants.Gas, octopusHalfHourlies);
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
            var sqlite = new SqLiteHelper(_account.Id, _logger!);

            var summary = sqlite.GetUsageInformation();
            var electric = summary.FirstOrDefault(s => s.FuelType == Constants.Electric);
            var gas = summary.FirstOrDefault(s => s.FuelType == Constants.Gas);

            if (electric != null)
            {
                _lastDateElectric = DateTime.ParseExact(electric.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            if (gas != null)
            {
                _lastDateGas = DateTime.ParseExact(gas.To, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
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
            ReadUsage.IsEnabled = state;
            ReadCosts.IsEnabled = state;
            ChargingSessions.IsEnabled = state;
            ExportUsage.IsEnabled = state;
            CancelOperations.IsEnabled = !state;
        }

        private void ReadFromRegistry()
        {
            var key = Registry.CurrentUser.OpenSubKey(@$"SOFTWARE\{Constants.ApplicationName}");
            if (key != null)
            {
                AccountId.Text = key.GetValue("AccountId")?.ToString();
                ApiKey.Password = key.GetValue("Api-Key")?.ToString();
            }
        }

        private void WriteToRegistry()
        {
            var key = Registry.CurrentUser.CreateSubKey(@$"SOFTWARE\{Constants.ApplicationName}");

            key.SetValue("AccountId", AccountId.Text);
            key.SetValue("Api-Key", ApiKey.Password);
        }
    }
}