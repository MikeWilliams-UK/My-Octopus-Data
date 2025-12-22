using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OctopusData.Helpers;
using OctopusData.Models;
using OctopusData.Models.Account;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
                while (currentDay >= _supplyDateElectric)
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

                // Fetch Gas usage
                SetStatusText("Fetching Gas usage ...");

                currentDay = DateTime.UtcNow.Date;
                while (currentDay >= _supplyDateGas)
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

        private void OnClick_ReadMeterReadings(object sender, RoutedEventArgs e)
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
            AccountStatistics.ItemsSource = sqlite.GetUsageInformation();
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
            ReadMeterReadings.IsEnabled = state;
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