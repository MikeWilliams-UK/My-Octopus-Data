using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OctopusData.Helpers;
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

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json")
                .Build();

            _httpHelper = new HttpHelper(_configuration);
        }

        private void OnLoaded_MainWindow(object sender, RoutedEventArgs e)
        {
            ReadFromRegistry();
        }

        private void OnClick_Login(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);

            if (string.IsNullOrEmpty(AccountId.Text) && string.IsNullOrEmpty(ApiKey.Password))
            {
                MessageBox.Show("Account Id and/or Api Key are blank", "Input Error");
            }
            else
            {
                WriteToRegistry();

                SetStatusText("Connecting ...");

                if (_httpHelper.Login(AccountId.Text, ApiKey.Password))
                {
                    Login.IsEnabled = false;
                }
            }
        }

        private void OnSelectionChanged_StopWhen(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OnClick_ReadUsage(object sender, RoutedEventArgs e)
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