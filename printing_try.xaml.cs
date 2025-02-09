using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Drawing.Printing;
using Aspose.Pdf;
using Aspose.Pdf.Devices;

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

        private DispatcherTimer _timer;

        public printing_try(string filePath, string fileName, string pageSize, int pageCount,
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

            // Start the printing task asynchronously
            Task.Run(() => StartPrintingAsync(FilePath));
        }

        private async Task StartPrintingAsync(string filePath)
        {
            try
            {
                Dispatcher.Invoke(() => ShowLoading(true));
               
                await Task.Delay(10000);

                await Task.Run(() => PrintPdfFile(filePath));

                Dispatcher.Invoke(() =>
                {
                    ShowLoading(false);
                    MessageBox.Show("Printing started successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigateToHomeUserControl();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ShowLoading(false);
                    MessageBox.Show($"An error occurred while printing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigateToHomeUserControl(); // Navigate to HomeUserControl even if there's an error
                });
            }
        }

        public void PrintPdfFile(string filePath)
        {
            Dispatcher.Invoke(() => ShowLoading(true));

            try
            {
                Document pdfDocument = new Document(filePath);
                PrinterSettings printerSettings = new PrinterSettings
                {
                    PrinterName = "EPSON L3110 Series"
                };

                // Set printer settings based on ColorStatus
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
                    if (pageIndex >= 1 && pageIndex <= pdfDocument.Pages.Count)
                    {
                        for (int copyIndex = 0; copyIndex < CopyCount; copyIndex++)
                        {
                            using (MemoryStream pageStream = new MemoryStream())
                            {
                                Resolution resolution = new Resolution(300);

                                if (ColorStatus.ToLower() == "colored")
                                {
                                    JpegDevice jpegDevice = new JpegDevice(resolution);
                                    jpegDevice.Process(pdfDocument.Pages[pageIndex], pageStream);
                                }
                                else if (ColorStatus.ToLower() == "greyscale")
                                {
                                    PngDevice pngDevice = new PngDevice(resolution);
                                    pngDevice.Process(pdfDocument.Pages[pageIndex], pageStream);
                                }
                                else
                                {
                                    JpegDevice jpegDevice = new JpegDevice(resolution);
                                    jpegDevice.Process(pdfDocument.Pages[pageIndex], pageStream);
                                }

                                PrintPage(pageStream, printerSettings);
                            }
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Page {pageIndex} is out of range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"An error occurred while printing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                Dispatcher.Invoke(() => ShowLoading(false));
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

        private void PrintPage(Stream pageStream, PrinterSettings printerSettings)
        {
            using (var image = System.Drawing.Image.FromStream(pageStream))
            {
                PrintDocument printDocument = new PrintDocument
                {
                    PrinterSettings = printerSettings
                };

                printDocument.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawImage(image, e.PageBounds);
                };

                printDocument.Print();
            }
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavigateToHomeUserControl()
        {
            // Logic to navigate to HomeUserControl
            // This could involve changing the content of a parent container, opening a new window, etc.
            // Example:
            var homeUserControl = new HomeUserControl();
            Application.Current.MainWindow.Content = homeUserControl;
            this.Close();
        }
    }
}
