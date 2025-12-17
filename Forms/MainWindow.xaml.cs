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
                _httpHelper = new HttpHelper(_configuration, AccountId.Text, ApiKey.Password);
                _httpHelper.SetLogger(_logger);

                SetMouseCursor();
                WriteToRegistry();

                SetStatusText("Connecting ...");

                var details = await _httpHelper.LoginAsync();
                if (details != null)
                {
                    _account.Id = AccountId.Text;

                    SetStatusText($"Logged in to Account {AccountId.Text}");
                    Login.IsEnabled = false;

                    if (details.Properties.Count == 1)
                    {
                        var property = details.Properties[0];

                        // Handle Electricity
                        foreach (var meterPoint in property.ElectricityMeterPoints)
                        {
                            Debug.WriteLine($"Electricity MPAN: {meterPoint.Mpan}");
                            _account.ElectricMpan = meterPoint.Mpan;

                            foreach (var meter in meterPoint.Meters)
                            {
                                Debug.WriteLine($"  Electric meter: {meter.SerialNumber}");
                                _account.ElectricMeterSerial = meter.SerialNumber;
                            }

                            foreach (var agreement in meterPoint.Agreements)
                            {
                                Debug.WriteLine($"  Electricity agreement {agreement.TariffCode} {agreement.ValidFrom} {agreement.ValidTo}");
                                if (agreement.ValidFrom < _supplyDateElectric)
                                {
                                    _supplyDateElectric = agreement.ValidFrom;
                                }
                            }
                        }


                        // Handle Gas
                        foreach (var meterPoint in property.GasMeterPoints)
                        {
                            Debug.WriteLine($"Gas MPRN: {meterPoint.Mprn}");
                            _account.GasMprn = meterPoint.Mprn;

                            foreach (var meter in meterPoint.Meters)
                            {
                                Debug.WriteLine($"  Gas meter: {meter.SerialNumber}");
                                _account.GasMeterSerial = meter.SerialNumber;
                            }

                            foreach (var agreement in meterPoint.Agreements)
                            {
                                Debug.WriteLine($"  Gas agreement {agreement.TariffCode} {agreement.ValidFrom} {agreement.ValidTo}");
                                if (agreement.ValidFrom < _supplyDateGas)
                                {
                                    _supplyDateGas = agreement.ValidFrom;
                                }
                            }
                        }
                    }

                    Debug.WriteLine($"E:{_supplyDateElectric} G:{_supplyDateGas}");
                }

                ClearDown();
                ShowAccountInfo();
            }
        }

        private async void OnClick_ReadUsage(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                var targetDate = _supplyDateElectric < _supplyDateGas
                                ? _supplyDateElectric
                                : _supplyDateGas;

                DateTime currentDay = DateTime.UtcNow.Date;
                while (currentDay >= targetDate)
                {
                    var electric = await _httpHelper.ObtainElectricHalfHourlyUsageAsync(_account, currentDay);
                    if (electric != null)
                    {
                        Debug.WriteLine($"Retrieved {electric.Results.Count} half-hourly records for today.");
                    }
                    var gas = await _httpHelper.ObtainGasHalfHourlyUsageAsync(_account, currentDay);
                    if (gas != null)
                    {
                        Debug.WriteLine($"Retrieved {gas.Results.Count} half-hourly records for today.");
                    }

                    // Go back in time one day
                    currentDay = currentDay.AddDays(-1);

                    break;
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

            ClearDown();
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

            SetStatusText("");
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