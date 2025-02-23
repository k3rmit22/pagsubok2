using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace kiosk_snapprint
{
    public partial class tray_connection_unique : UserControl
    {
        public byte[] FileBytes { get; }
        public string FileName { get; }
        public string PageSize { get; }
        public string ColorMode { get; }
        public List<int> SelectedPages { get; }
        public int CopyCount { get; }
        public int TotalPrice { get; }

        private SerialPort serialPort;

        public tray_connection_unique(byte[] fileBytes, string fileName, string pageSize, string colorMode, List<int> selectedPages, int copyCount, int totalPrice)
        {
            InitializeComponent();
            FileBytes = fileBytes;
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages;
            CopyCount = copyCount;
            TotalPrice = totalPrice;

            // Initialize SerialPort with the correct COM port and baud rate
            serialPort = new SerialPort("COM6", 9600, Parity.None, 8, StopBits.One);
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
            else if (PageSize == "Long")
            {
                pageSizeValue = 3; // Set to 3 if PageSize is Long
            }
            else if (PageSize == "Letter (Short)")
            {
                pageSizeValue = 1; // Set to 1 if PageSize is Letter (Short)
            }

            // Calculate the number of selected pages (this is just the count of SelectedPages)
            int numberOfSelectedPages = SelectedPages.Count;

            // Get the values of the selected pages (comma-separated string of page numbers)
            string selectedPagesString = string.Join(",", SelectedPages);

            // Calculate the total number of pages based on selected pages and copy count
            int totalPages = numberOfSelectedPages * CopyCount;

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
                await Task.Delay(5000);

                // Create a new instance of the Unique_for_printing window
                Unique_for_printing printingWindow = new Unique_for_printing(
                    fileBytes: FileBytes,
                    fileName: FileName,
                    pageSize: PageSize,
                    colorMode: ColorMode,
                    selectedPages: SelectedPages,
                    copyCount: CopyCount,
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
