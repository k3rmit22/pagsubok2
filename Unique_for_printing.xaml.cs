using System;
using System.IO;
using System.Drawing.Printing;
using System.Windows;
using PdfiumViewer;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using iText.Commons.Utils;
using System.Drawing.Imaging;

namespace kiosk_snapprint
{
    public partial class Unique_for_printing : Window
    {
        public byte[] FileBytes { get; }
        public string FileName { get; }
        public string PageSize { get; }
        public string ColorMode { get; }
        public List<int> SelectedPages { get; }
        public int CopyCount { get; }
        public double TotalPrice { get; }

        public string Action { get; private set; } // New property
        public decimal TotalAmount { get; private set; } // Include TransactionData.TotalAmount

        public Visibility OverlayVisibility
        {
            get { return LoadingOverlay.Visibility; }
            set { LoadingOverlay.Visibility = value; }
        }

        public Unique_for_printing(byte[] fileBytes, string fileName, string pageSize, string colorMode, List<int> selectedPages, int copyCount, double totalPrice)
        {
            InitializeComponent();

            // Set the values to class properties
            FileBytes = fileBytes;
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages;
            CopyCount = copyCount;
            TotalPrice = totalPrice;

            Action = "Proceed";
            TotalAmount = TransactionData.TotalAmount;

            Debug.WriteLine($"Printing - FileByte: {FileBytes}");
            Debug.WriteLine($"Printing - FileName: {FileName}");
            Debug.WriteLine($"Printing - PageSize: {PageSize}");
            Debug.WriteLine($"Printing - ColorMode: {ColorMode}");
            Debug.WriteLine($"Printing - SelectedPages: {SelectedPages}");
            Debug.WriteLine($"Printing - CopyCount: {CopyCount}");

            // Attach Loaded event handler
            this.Loaded += Unique_for_printing_Loaded;
        }

        // When the window is loaded, display the GIF and start the operations
        private async void Unique_for_printing_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                OverlayVisibility = Visibility.Visible; // Show loading overlay

                // Perform printing and send transaction data
                await PrintDocumentAsync();
                await SendTransactionDataAsync(FileName, PageSize, SelectedPages, ColorMode, CopyCount, TotalPrice, Action, TotalAmount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                ResetSystem(FileName, PageSize, SelectedPages, ColorMode, CopyCount, TotalPrice, Action);
              
                OverlayVisibility = Visibility.Collapsed; // Hide loading overlay
                NavigateToHome();
                this.Close(); // Close the window after all operations
            }
        }

        // Asynchronous method to print the document
        private async Task PrintDocumentAsync()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(FileBytes))
                using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream))
                {
                    PrinterSettings printerSettings = new PrinterSettings
                    {
                        PrinterName = "EPSON L3110 Series" // Set your printer's name here
                    };

                    PrintDocument printDoc = pdfDocument.CreatePrintDocument();

                    // Configure printer settings for color mode
                    if (ColorMode.Equals("greyscale", StringComparison.OrdinalIgnoreCase))
                    {
                        SetPrinterSettingsToGreyscale(printerSettings);
                    }
                    else if (ColorMode.Equals("colored", StringComparison.OrdinalIgnoreCase))
                    {
                        SetPrinterSettingsToColored(printerSettings);
                    }

                    printDoc.PrinterSettings = printerSettings;

                    // Loop through selected pages and handle copy count
                    foreach (var pageNum in SelectedPages)
                    {
                        if (pageNum < 1 || pageNum > pdfDocument.PageCount)
                            continue; // Skip invalid page numbers

                        for (int i = 0; i < CopyCount; i++)
                        {
                            printDoc.PrintController = new StandardPrintController(); // Suppress print dialogs
                            printDoc.DefaultPageSettings.PrinterSettings = printerSettings;

                            // Set current page to print
                            printDoc.PrinterSettings.FromPage = pageNum;
                            printDoc.PrinterSettings.ToPage = pageNum;

                            printDoc.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during printing: {ex.Message}");
            }
        }

        // Method to send transaction data to the database
        private async Task SendTransactionDataAsync(string fileName, string pageSize, List<int> selectedPages,
                                                    string colorMode, int copyCount, double totalPrice,
                                                    string action, decimal totalAmount)
        {
            string url = "https://snapprintadmin.online/transaction.php"; // Your API endpoint

            var transactionData = new
            {
                FileName = fileName,
                PageSize = pageSize,
                SelectedPages = selectedPages,
                ColorStatus = colorMode,
                CopyCount = copyCount,
                Action = action,
                TotalPrice = totalPrice,
                TotalAmount = totalAmount // Inserted amount
            };

            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(transactionData);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    string responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {responseString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending transaction data: {ex.Message}");
                }
            }
        }

        // Method to set the printer to grayscale
        private void SetPrinterSettingsToGreyscale(PrinterSettings printerSettings)
        {
            printerSettings.DefaultPageSettings.Color = false;
        }

        // Method to set the printer to color
        private void SetPrinterSettingsToColored(PrinterSettings printerSettings)
        {
            printerSettings.DefaultPageSettings.Color = true;
        }

        // Method to navigate back to Home UserControl
        private void NavigateToHome()
        {
            // Assuming MainWindow contains a method called NavigateToUserControl
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToUserControl(new HomeUserControl());
            }
        }

        private void ResetSystem(string FileName, string PageSize, List<int> SelectedPages,
                                                    string ColorMode, int CopyCount, double TotalPrice,
                                                    string Action)
        {
            // Clear transaction data
            TransactionData.Reset();

            // Reset any UI elements or variables if needed
            FileName = string.Empty;
            PageSize = string.Empty;
            SelectedPages.Clear();
            ColorMode = string.Empty;
            CopyCount = 0;
            TotalPrice = 0;
            Action = string.Empty;


            // Refresh UI by forcing layout updates if necessary
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Force UI update
                Window mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Content = new HomeUserControl(); // Reload HomeUserControl
                }
            });
        }

    }
}
