using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;


namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for Tray_connection_qr.xaml
    /// </summary>
    public partial class Tray_connection_qr : UserControl
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string PageSize { get; private set; }
        public int PageCount { get; private set; }
        public string ColorStatus { get; private set; }
        public int NumberOfSelectedPages { get; private set; }
        public int CopyCount { get; private set; }
        public List<int> SelectedPages { get; private set; }
        public double TotalPrice { get; private set; }

        private SerialPort serialPort;

        public Tray_connection_qr(string filePath, string fileName, string pageSize, int pageCount,
                              string colorStatus, int numberOfSelectedPages, int copyCount,
                              List<int> selectedPages, double totalPrice)
        {
            InitializeComponent();
            // Store passed values
            FilePath = filePath;
            FileName = fileName;
            PageSize = pageSize;
            PageCount = pageCount;
            ColorStatus = colorStatus;
            NumberOfSelectedPages = numberOfSelectedPages;
            CopyCount = copyCount;
            SelectedPages = selectedPages;
            TotalPrice = totalPrice;

            // Initialize SerialPort with the correct COM port and baud rate
            serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            serialPort.Open();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            filename.Text = FileName;
            progressBar.IsIndeterminate = true;

            // Calculate page size value
            int pageSizeValue = 3; // Default value for other page sizes
            if (PageSize == "A4")
            {
                pageSizeValue = 2; // Set to 2 if PageSize is A4
            }
            else if (PageSize == "Legal (Long)")
            {
                pageSizeValue = 3; // Set to 3 if PageSize is Long
            }

            // Calculate the total number of pages based on NumberOfSelectedPages and CopyCount
            int totalPages = NumberOfSelectedPages * CopyCount;

            // Build the command string for normal tray selection and page count
            string command = $"{pageSizeValue},{totalPages}";

            // Check if the user requested a "Low" command (e.g., for reversing lifter motor)
            if (PageSize == "A4" || PageSize == "Long")
            {
                // If "Low" command is triggered, the command format changes to tray number + ",Low"
                if (CommandLowIsRequested()) // Implement your own condition to detect if "Low" is requested
                {
                    command = $"{pageSizeValue},Low";
                }
            }

            // Send the command to the hardware via COM port
            SendCommandToHardware(command);
        }

        private bool CommandLowIsRequested()
        {
            // Define a condition to check if the "Low" command is needed.
            // For example, check if a button is clicked or a certain condition is met
            return false;  // Placeholder, replace with actual logic
        }

        private void SendCommandToHardware(string command)
        {
            try
            {
                // Send command to the Arduino over the serial port
                if (serialPort.IsOpen)
                {
                    serialPort.WriteLine(command); // Send the command to Arduino
                    Debug.WriteLine($"Command Sent: {command}");
                    NavigateTotrayconnection();
                }
                else
                {
                    Debug.WriteLine("Serial port is not open.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending command: {ex.Message}");
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up: close the serial port when the UserControl is unloaded
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Debug.WriteLine("Serial port closed.");
            }
        }

        private async void NavigateTotrayconnection()
        {
            try
            {
                // Introduce a 3-second delay
                await Task.Delay(3000);

                // Create a new instance of the printing_try window
                printing_try printingWindow = new printing_try(
                    filePath: FilePath,
                    fileName: FileName,
                    pageSize: PageSize,
                    pageCount: PageCount,
                    colorStatus: ColorStatus,
                    numberOfSelectedPages: NumberOfSelectedPages,
                    copyCount: CopyCount,
                    selectedPages: SelectedPages,
                    totalPrice: TotalPrice
                );

                // Set the owner of the modal to the current main window
                printingWindow.Owner = Application.Current.MainWindow;

                // Show the window as a modal
                printingWindow.ShowDialog();

                // After modal closes, you can perform additional logic here if needed
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




    }
}
