﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Drawing.Printing;

using System.Net.Http;
using System.Text;
using PdfiumViewer;
using iText.Commons.Utils;
using System.Diagnostics;
using System.Drawing.Imaging;



namespace kiosk_snapprint
{
    public partial class printing_try : Window
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

        public string Action { get; private set; } // New property

        public decimal TotalAmount { get; private set; } // Include TransactionData.TotalAmount

        private DispatcherTimer _timer;

        public printing_try(string filePath, string fileName, string pageSize, int pageCount,
                            string colorStatus, int numberOfSelectedPages, int copyCount,
                            List<int> selectedPages, double totalPrice)
        {
            InitializeComponent();

            TotalAmount = TransactionData.TotalAmount;

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

            Action = "Proceed";

            // Start the printing task asynchronously
            Task.Run(() => StartPrintingAsync(FilePath));
        }

        //lagyan ng timer tanggalin yung message box
        private async Task StartPrintingAsync(string filePath)
        {
            try
            {
                Dispatcher.Invoke(() => ShowLoading(true)); // Show the loading overlay

                await Task.Delay(300); // Optional delay for smoother UI update

                await Task.Run(() => PrintPdfFile(filePath)); // Run the printing task

                // Pass all required parameters to SendTransactionDataAsync
                await SendTransactionDataAsync(
                    FileName,
                    PageSize,
                    SelectedPages,
                    ColorStatus,
                    CopyCount,
                    TotalPrice,
                    Action,
                    TotalAmount
                ); // Send transaction data after printing

                await UpdatePaperStockAsync(PageSize, SelectedPages, CopyCount);

                await Task.Delay(500); // Allow UI to process before resetting
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
               

                Dispatcher.BeginInvoke(() =>
                {
                    ShowLoading(false); // Hide the loading overlay
                    NavigateToHomeUserControl(); // Navigate to home after reset
                });
            }
        }
      




        public void PrintPdfFile(string filePath)
        {
            try
            {
                using (var pdfDocument = PdfiumViewer.PdfDocument.Load(filePath)) // Load the PDF
                {
                    PrinterSettings printerSettings = new PrinterSettings
                    {
                        PrinterName = "EPSON L3110 Series"
                    };

                    // Configure printer settings for color/greyscale
                    if (ColorStatus.ToLower() == "greyscale")
                    {
                        SetPrinterSettingsToGreyscale(printerSettings);
                    }
                    else if (ColorStatus.ToLower() == "colored")
                    {
                        SetPrinterSettingsToColored(printerSettings);
                    }

                    foreach (var pageIndex in SelectedPages)
                    {
                        if (pageIndex >= 1 && pageIndex <= pdfDocument.PageCount)
                        {
                            for (int copyIndex = 0; copyIndex < CopyCount; copyIndex++)
                            {
                                using (var printDocument = pdfDocument.CreatePrintDocument())
                                {
                                    printDocument.PrinterSettings = printerSettings;

                                    // Set specific page range for printing
                                    printDocument.PrinterSettings.FromPage = pageIndex;
                                    printDocument.PrinterSettings.ToPage = pageIndex;

                                    printDocument.Print(); // Print the selected page
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException($"Page index {pageIndex} is out of range.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Printing Error: {ex.Message}");
            }
        }




        private void SetPrinterSettingsToGreyscale(PrinterSettings printerSettings)
        {
            // Modify the printer settings to greyscale
            printerSettings.DefaultPageSettings.Color = false; // Set to greyscale
        }

        private void SetPrinterSettingsToColored(PrinterSettings printerSettings)
        {
            // Modify the printer settings to color
            printerSettings.DefaultPageSettings.Color = true; // Set to colored
        }




        private void ShowLoading(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            LoadingOverlay.IsHitTestVisible = isLoading; // Disable interaction when hidden
        }


        private async Task SendTransactionDataAsync(string fileName, string pageSize, List<int> selectedPages,
                                             string colorStatus, int copyCount, double totalPrice,
                                             string action, decimal totalAmount)
        {
            // Define your PHP API URL
            string url = "https://snapprintadmin.online/transaction.php"; // Replace with your actual API endpoint

            // Prepare the JSON payload using the class properties
            var transactionData = new
            {
                FileName = fileName,
                PageSize = pageSize,
                SelectedPages = selectedPages,
                ColorStatus = colorStatus,
                CopyCount = copyCount,
                Action = action,
                TotalPrice = totalPrice,
                TotalAmount = totalAmount  //inserted amount  
            };

            // Serialize the object to JSON format
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(transactionData);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Create HTTP content with the serialized JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the POST request to the API endpoint
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Read the API response
                    string responseString = await response.Content.ReadAsStringAsync();

                    // (Optional) Log or display the response for debugging
                    Console.WriteLine($"API Response: {responseString}");
                }
                catch (Exception ex)
                {
                    // Handle errors during the API call
                    Console.WriteLine($"Error sending transaction data: {ex.Message}");
                }
            }
        }

        private async Task UpdatePaperStockAsync(string pageSize, List<int> selectedPages, int copyCount)
        {
            // Define your updated PHP API URL for paper stock deduction
            string url = "https://snapprintadmin.online/update_paper_stock.php"; // Replace with your actual API endpoint

            // Prepare the JSON payload with only the necessary data
            var paperStockData = new
            {
                PageSize = pageSize,
                SelectedPages = selectedPages,
                CopyCount = copyCount
            };

            // Serialize the object to JSON format
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(paperStockData);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Create HTTP content with the JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the POST request to the API endpoint
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Read the API response
                    string responseString = await response.Content.ReadAsStringAsync();

                    // Debugging output
                    Console.WriteLine($"Paper Stock API Response: {responseString}");
                }
                catch (Exception ex)
                {
                    // Handle errors
                    Console.WriteLine($"Error updating paper stock: {ex.Message}");
                }
            }
        }

        private void ResetSystem(string FileName, string PageSize, List<int> SelectedPages,
                                                  string ColorStatus, int CopyCount, double TotalPrice,
                                                  string Action)
        {
            // Clear transaction data
            TransactionData.Reset();

            // Reset any UI elements or variables if needed
            FileName = string.Empty;
            PageSize = string.Empty;
            SelectedPages.Clear();
            ColorStatus = string.Empty;
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


        

        private void NavigateToHomeUserControl()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Replace `HomeUserControl` with the appropriate UserControl for your home page
            mainWindow.MainContent.Content = new HomeUserControl();
            this.Close();
        }



    }
}
