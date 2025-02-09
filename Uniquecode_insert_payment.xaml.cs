using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Threading;
using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using System.Text.RegularExpressions;
using MailKit.Search;



namespace kiosk_snapprint
{

    public partial class Uniquecode_insert_payment : UserControl
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string PageSize { get; set; }
        public string ColorMode { get; set; }
        public List<int> SelectedPages { get; set; }
        public int CopyCount { get; set; }
        public int TotalPrice { get; set; }

        private SerialPort _serialPort; // For communication with payment hardware
        private SerialPort _secondSerialPort; // For communication with second hardware (e.g., servo)

        private int _insertedAmount;


        private const string gmailHost = "imap.gmail.com";
        private const int gmailPort = 993;
        private const string username = "justinyuji413@gmail.com"; // Replace with your email
        private const string password = "zmpc uspg csro xlpi"; // Replace with your app password

        private DispatcherTimer emailCheckTimer;

        public Uniquecode_insert_payment(byte[] fileBytes, string fileName, string pageSize, string colorMode, List<int> selectedPages, int copyCount, int totalPrice)
        {
            InitializeComponent();
            FileBytes = fileBytes;
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages;
            CopyCount = copyCount;
            TotalPrice = totalPrice;


            Loadsummary(FileName, TotalPrice);
            InitializeSerialPorts(); // Initialize serial ports
            ResetInsertedAmount(); // Initialize the reset logic
            this.Unloaded += UserControl_Unloaded;

            // Start the email checking task
            StartEmailChecking();
        }

        private void Loadsummary(string fileName, double totalPrice)
        {
            name_label.Text = fileName ?? "Unknown File";
            total_label.Text = $"{totalPrice:F2}";
        }

        private void StartEmailChecking()
        {
            emailCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // Check emails every 5 seconds
            };
            emailCheckTimer.Tick += async (sender, e) =>
            {
                await CheckForPaymentEmail();
            };
            emailCheckTimer.Start();
        }

        private async Task CheckForPaymentEmail()
        {
            try
            {
                using (var client = new ImapClient())
                {
                    await client.ConnectAsync(gmailHost, gmailPort, true);
                    await client.AuthenticateAsync(username, password);

                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite); // Open in ReadWrite mode to mark emails as seen

                    // Search for unread emails containing the GCash payment message
                    var query = SearchQuery.BodyContains("You have received the money in GCash!").And(SearchQuery.NotSeen);
                    var results = await inbox.SearchAsync(query);

                    if (results.Count > 0)
                    {
                        var message = await inbox.GetMessageAsync(results[results.Count - 1]); // Get the latest message
                        string body = message.TextBody;

                        // Parse the email body to find the inserted amount
                        double amount = ParseAmountFromEmailBody(body);
                        if (amount > 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _insertedAmount += (int)amount;
                                inserted_amount_label.Text = $"{_insertedAmount:F2}";
                                Debug.WriteLine($"Inserted amount updated: {_insertedAmount:F2}");
                                CheckForPaymentCompletion();
                            });

                            // Mark the email as read (Seen) so it is not retrieved again
                            await inbox.AddFlagsAsync(results[results.Count - 1], MessageFlags.Seen, true);
                            Debug.WriteLine("Marked email as read (Seen).");
                        }
                    }

                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking email: {ex.Message}");
            }
        }


        // Method to parse the inserted amount from the email body
        private double ParseAmountFromEmailBody(string body)
        {
            try
            {
                // Look for the pattern of the amount, assuming it's a numeric value after the phrase
                var match = Regex.Match(body, @"\b(\d+(\.\d{1,2})?)\b"); // Match numbers like 100, 100.50, etc.
                if (match.Success)
                {
                    return double.Parse(match.Value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing email body: {ex.Message}");
            }
            return 0;
        }

        private void InitializeSerialPorts()
        {
            try
            {
                // Initialize payment serial port
                _serialPort = new SerialPort("COM8", 115200);
                _serialPort.DataReceived += SerialPort_DataReceived;

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    Debug.WriteLine("Payment serial port connected successfully.");
                }

                // Initialize second hardware serial port (e.g., servo)
                _secondSerialPort = new SerialPort("COM9", 115200);

                if (!_secondSerialPort.IsOpen)
                {
                    _secondSerialPort.Open();
                    Debug.WriteLine("Second hardware serial port connected successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to the serial ports: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadLine().Trim();

                if (int.TryParse(data, out int amount))
                {
                    if (amount >= 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _insertedAmount = amount;
                            inserted_amount_label.Text = $"{_insertedAmount:F2}";
                            Debug.WriteLine($"Amount updated: {_insertedAmount}");

                            CheckForPaymentCompletion();
                        });
                    }
                }
                else
                {
                    Debug.WriteLine($"Invalid data received: {data}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading serial port data: {ex.Message}");
            }
        }

        private bool paymentCompleted = false;

        private void CheckForPaymentCompletion()
        {
            // Check if the inserted amount meets or exceeds the total price and payment hasn't been completed already
            if (_insertedAmount >= TotalPrice && !paymentCompleted)
            {
                // Mark payment as completed to prevent multiple triggers
                paymentCompleted = true;

                // Send command to servo to move to 180 degrees
                SendServoCommand("servo0");

                // Proceed to the next step (navigate to the printing window)
                NavigateTotrayconnection();
            }
        }

        private void SendServoCommand(string command)
        {
            try
            {
                if (_secondSerialPort != null && _secondSerialPort.IsOpen)
                {
                    _secondSerialPort.WriteLine(command); // Send the command to the second hardware (e.g., servo)
                    Debug.WriteLine($"Sent command to servo: {command}");
                }
                else
                {
                    Debug.WriteLine("Second serial port is not open. Cannot send command.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending servo command: {ex.Message}");
            }
        }

        //private async void NavigateTotrayconnection()
        //{
        //    try
        //    {
        //        // Introduce a 3-second delay
        //        await Task.Delay(3000);

        //        // Create a new instance of the printing_try window
        //        Unique_for_printing printingWindow = new Unique_for_printing(

        //           fileBytes: FileBytes,
        //           fileName: FileName,
        //           pageSize: PageSize,
        //           colorMode: ColorMode,
        //           selectedPages: SelectedPages,
        //           copyCount: CopyCount,
        //           totalPrice: TotalPrice
        //       );

        //        // Set the owner of the modal to the current main window
        //        printingWindow.Owner = Application.Current.MainWindow;

        //        // Show the window as a modal
        //        printingWindow.ShowDialog();

        //        // After modal closes, you can perform additional logic here if needed
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any errors that occur
        //        MessageBox.Show($"An error occurred while opening the printing window: {ex.Message}",
        //                        "Error",
        //                        MessageBoxButton.OK,
        //                        MessageBoxImage.Error);
        //    }
        //}

        private async void NavigateTotrayconnection()
        {
            try
            {
                // Introduce a 3-second delay
                await Task.Delay(3000);

                // Create a new instance of the tray_connection_unique UserControl
                tray_connection_unique tray_connection = new tray_connection_unique(
                    fileBytes: FileBytes,
                    fileName: FileName,
                    pageSize: PageSize,
                    colorMode: ColorMode,
                    selectedPages: SelectedPages,
                    copyCount: CopyCount,
                    totalPrice: TotalPrice
                );

                // Set the UserControl as the content of the ContentControl in your main window
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainContent.Content = tray_connection;

                // Optionally, perform additional actions if needed after the control is loaded
            }
            catch (Exception ex)
            {
                // Handle any errors that occur
                MessageBox.Show($"An error occurred while opening the printing window: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }



        private void ResetInsertedAmount()
        {
            try
            {
                _insertedAmount = 0; // Reset the C# application state
                inserted_amount_label.Text = $"{_insertedAmount:F2}";

                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("RESET"); // Send reset command to Arduino
                    Debug.WriteLine("Reset command sent to Arduino.");
                }
                else
                {
                    Debug.WriteLine("Serial port is not open. Cannot send reset command.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during reset: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.Dispose();
                    _serialPort = null;

                    Debug.WriteLine("Payment serial port disposed and event handler removed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing payment serial port: {ex.Message}");
                }
            }

            if (_secondSerialPort != null)
            {
                try
                {
                    if (_secondSerialPort.IsOpen)
                    {
                        _secondSerialPort.Close();
                    }

                    _secondSerialPort.Dispose();
                    _secondSerialPort = null;

                    Debug.WriteLine("Second serial port disposed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing second serial port: {ex.Message}");
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("UserControl_Unloaded triggered.");
            Dispose();
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and display the cancel transaction modal
            cancel_transaction_modal cancelModal = new cancel_transaction_modal
            {
                Owner = Application.Current.MainWindow // Set the main window as the owner
            };

            // Pass the second serial port (COM9) to the modal so it can send a command if necessary
            cancelModal.SecondSerialPort = _secondSerialPort;

            // Show the modal dialog (blocks further interaction until closed)
            cancelModal.ShowDialog();
        }
    }
}
