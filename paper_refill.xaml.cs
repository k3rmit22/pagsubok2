using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for paper_refill.xaml
    /// </summary>
    public partial class paper_refill : UserControl
    {
        private static Dictionary<int, string> lastSentCommands = new Dictionary<int, string>();
        private static SerialPort serialPort;
        private static bool isSerialPortOpen = false;  // Flag to track port status

        public paper_refill()
        {
            InitializeComponent();

            // Initialize the last sent commands for lifters
            for (int lifterId = 1; lifterId <= 3; lifterId++)
            {
                lastSentCommands[lifterId] = null;
            }

            // Open serial port only once
            OpenSerialPort("COM6", 9600);

            // Start fetching commands asynchronously
            StartFetchingCommands();
        }

        /// <summary>
        /// Opens the serial port and keeps it open until navigation or app closure.
        /// </summary>
        private void OpenSerialPort(string portName, int baudRate)
        {
            if (serialPort == null)
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    DtrEnable = true,         // Data Terminal Ready
                    RtsEnable = true,         // Request to Send
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                try
                {
                    serialPort.Open();
                    isSerialPortOpen = true;
                    Debug.WriteLine($"Serial port {portName} opened successfully.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open serial port: {ex.Message}");
                    MessageBox.Show($"Failed to open serial port: {ex.Message}", "Serial Port Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                Debug.WriteLine("Serial port already open.");
            }
        }

        /// <summary>
        /// Starts continuously fetching lifter commands every 2 minutes.
        /// </summary>
        private async void StartFetchingCommands()
        {
            Debug.WriteLine("Starting to fetch lifter commands...");

            while (true)
            {
                await FetchAllCommands();
                await Task.Delay(500); // Fetch commands every 2 minutes
            }
        }

        /// <summary>
        /// Iterates over all lifters and fetches their commands.
        /// </summary>
        private async Task FetchAllCommands()
        {
            for (int lifterId = 1; lifterId <= 3; lifterId++)
            {
                await FetchAndProcessCommand(lifterId);
            }
        }

        /// <summary>
        /// Fetches and processes the command for a specific lifter.
        /// </summary>
        /// <param name="lifterId">Lifter ID</param>
        private async Task FetchAndProcessCommand(int lifterId)
        {
            string apiUrl = $"https://snapprintadmin.online/command.php?id={lifterId}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Lifter {lifterId} command: {jsonResponse}");

                        // Extract the command properly
                        string rawCommand = jsonResponse.Replace("{\"command\":\"", "").Replace("\"}", "").Trim();

                        if (!string.IsNullOrEmpty(rawCommand) && lastSentCommands[lifterId] != rawCommand)
                        {
                            lastSentCommands[lifterId] = rawCommand;
                            SendCommandToArduino(rawCommand);
                            UpdateLifterStatus(lifterId, rawCommand);
                        }
                        else
                        {
                            Debug.WriteLine($"Lifter {lifterId}: No new command or invalid command.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to fetch command for Lifter {lifterId}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching command for Lifter {lifterId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sends the command to the Arduino through the serial port.
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommandToArduino(string command)
        {
            if (isSerialPortOpen && serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.WriteLine(command);
                    Debug.WriteLine($"Command sent to Arduino: {command}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending command to Arduino: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("Serial port is closed or unavailable.");
            }
        }

        /// <summary>
        /// Updates the UI with the current lifter status.
        /// </summary>
        /// <param name="lifterId">Lifter ID</param>
        /// <param name="command">Command received</param>
        private void UpdateLifterStatus(int lifterId, string command)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"Updating UI for Lifter {lifterId} with command: {command}");

                switch (lifterId)
                {
                    case 1:
                        Lifter1Status.Text = $"Lifter 1: {command}";
                        break;
                    case 2:
                        Lifter2Status.Text = $"Lifter 2: {command}";
                        break;
                    case 3:
                        Lifter3Status.Text = $"Lifter 3: {command}";
                        break;
                }
            });
        }

        /// <summary>
        /// Properly closes the serial port when navigating away or closing the WPF app.
        /// </summary>
        private void CloseSerialPort()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    isSerialPortOpen = false;
                    Debug.WriteLine("Serial port closed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error closing serial port: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles unloading event by closing the serial port properly.
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseSerialPort();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Replace `HomeUserControl` with the appropriate UserControl for your home page
            mainWindow.MainContent.Content = new HomeUserControl();
        }
    }
}
