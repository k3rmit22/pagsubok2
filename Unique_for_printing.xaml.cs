using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

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

        // Add properties for overlay visibility
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
            Debug.WriteLine($"Printing - FileByte: {FileBytes}");
            Debug.WriteLine($"Printing - FileName: {FileName}");
            Debug.WriteLine($"Printing - Pagesize: {PageSize}");
            Debug.WriteLine($"Printing - colormode: {ColorMode}");
            Debug.WriteLine($"Printing - selectedPages: {SelectedPages}");
            Debug.WriteLine($"Printing - CopyCount: {CopyCount}");



            // Attach Loaded event handler
            this.Loaded += Unique_for_printing_Loaded;
        }

        // When the window is loaded, call the PrintDocument method
        private async void Unique_for_printing_Loaded(object sender, RoutedEventArgs e)
        {
            OverlayVisibility = Visibility.Visible; // Show overlay
            await PrintDocumentAsync(); // Call the asynchronous print method
        }

        // Asynchronous method to print the document
        // Assuming MainWindow has a method to navigate to different UserControls
        private async Task PrintDocumentAsync()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(FileBytes))
                {
                    Document pdfDocument = new Document(stream); // Load the PDF from byte array

                    // Set up the printer settings
                    PrinterSettings printerSettings = new PrinterSettings
                    {
                        PrinterName = "EPSON L3110 Series" // Set your printer's name here
                    };

                    // Set printer settings based on the color mode
                    if (ColorMode.Equals("greyscale", StringComparison.OrdinalIgnoreCase))
                    {
                        SetPrinterSettingsToGreyscale(printerSettings); // Set the printer to greyscale
                    }
                    else if (ColorMode.Equals("colored", StringComparison.OrdinalIgnoreCase))
                    {
                        SetPrinterSettingsToColored(printerSettings); // Set the printer to colored
                    }

                    // Loop through each selected page and handle the copy count manually
                    foreach (var pageNum in SelectedPages)
                    {
                        if (pageNum < 1 || pageNum > pdfDocument.Pages.Count)
                            continue; // Skip invalid page numbers

                        // Render the page as an image asynchronously
                        await Task.Run(() =>
                        {
                            using (MemoryStream imageStream = new MemoryStream())
                            {
                                Resolution resolution = new Resolution(300); // Set the DPI
                                JpegDevice jpegDevice = new JpegDevice(resolution, 100); // Render as JPEG
                                jpegDevice.Process(pdfDocument.Pages[pageNum], imageStream);
                                imageStream.Position = 0;

                                using (System.Drawing.Image pageImage = System.Drawing.Image.FromStream(imageStream))
                                {
                                    // Print the page based on the copy count
                                    for (int i = 0; i < CopyCount; i++)
                                    {
                                        PrintPage(printerSettings, pageImage); // Print one copy
                                    }
                                }
                            }
                        });
                    }

                    // Print completed message
                    NavigateToHome();
                }
            }
            catch (Exception ex)
            {
                NavigateToHome();
            }
            finally
            {
                // Hide the overlay after printing is done
                OverlayVisibility = Visibility.Collapsed;

                // Navigate back to the home UserControl
                NavigateToHome();

                // Close the current window
                this.Close();
            }
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



        // Method to print a single page image
        private void PrintPage(PrinterSettings printerSettings, System.Drawing.Image pageImage)
        {
            PrintDocument printDoc = new PrintDocument
            {
                PrinterSettings = printerSettings
            };

            printDoc.PrintPage += (sender, e) =>
            {
                e.Graphics.DrawImage(pageImage, e.PageBounds);
            };

            printDoc.Print();
        }


        // Method to set the printer to grayscale
        private void SetPrinterSettingsToGreyscale(PrinterSettings printerSettings)
        {
            // Modify the printer settings to greyscale
            printerSettings.DefaultPageSettings.Color = false; // Set to greyscale
        }

        // Method to set the printer to color
        private void SetPrinterSettingsToColored(PrinterSettings printerSettings)
        {
            // Modify the printer settings to color
            printerSettings.DefaultPageSettings.Color = true; // Set to colored
        }


    }
}
