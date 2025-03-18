using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PdfiumViewer;

using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SD = System.Drawing;
using SWC = System.Windows.Controls;
using System.Drawing;


namespace kiosk_snapprint
{
    
    public partial class Pricing_for_Unique : UserControl
    {

        // Public properties to hold the values passed from uniquePreferences
        public string FileName { get; set; }
        public string PageSize { get; set; }
        public string ColorMode { get; set; }
        public List<int> SelectedPages { get; set; }
        public int CopyCount { get; set; }
        public byte[] FileBytes { get; set; } // Added property for fileBytes

        public double TotalPrice { get; set; }

        public uniquePreferences _previousPage;  // Field to hold reference to the previous page


        public Pricing_for_Unique(string fileName, string pageSize, string colorMode, List<int> selectedPages, int copyCount, byte[] fileBytes)
        {
            InitializeComponent();
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages;
            CopyCount = copyCount;
            FileBytes = fileBytes;

            // Update the UI or perform any other operations with the passed values
            // For example, you could display these values on the UI
            Debug.WriteLine($"Pricing_for_unique - FileByte: {FileBytes}");
            Debug.WriteLine($"Pricing_for_unique - FileName: {FileName}");
            Debug.WriteLine($"Pricing_for_unique - Pagesize: {PageSize}");
            Debug.WriteLine($"Pricing_for_unique - colormode: {ColorMode}");
            Debug.WriteLine($"Pricing_for_unique - selectedPages: {SelectedPages}");
            Debug.WriteLine($"Pricing_for_unique - CopyCount: {CopyCount}");
        }
        

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate back to the uniquePreferences page
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainContent.Content = _previousPage;  // _previousPage is passed from uniquePreferences
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating back: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Loaded event handler
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("LoadSummary CALLED!");
            string normalizedPageSize = NormalizePageSize(PageSize);
            string normalizedColorStatus = NormalizeColorStatus(ColorMode);

            // Set the UI elements with the values passed in the constructor
            filename.Text = FileName; // Display the file name
            color_label.Text = ColorMode; // Display the color mode
            pagesize_label.Text = PageSize;
            selected_pages_label.Text = string.Join(", ", SelectedPages); // Display the selected pages as a comma-separated list
            Copies_label.Text = CopyCount.ToString(); // Display the copy count


                // Use colorModeValue in calculations
                double totalPrice = CalculateTotalPrice(FileBytes, ColorMode, SelectedPages, CopyCount);
                TotalPrice = totalPrice;
                // Display the calculated total price (ensure TotalPriceLabel exists in the UI)
                total_label.Text = totalPrice.ToString();

        }


        private double CalculateTotalPrice(byte[] fileBytes, string colorStatus, List<int> selectedPages, int copyCount)
        {
            Debug.WriteLine("ComputeTotalPriceForUnique CALLED!");
            if (selectedPages == null || selectedPages.Count == 0)
            {
                Debug.WriteLine("Error: No pages selected for printing.");
                return 0.0;
            }

            double totalCost = 0.0;

            using (var pdfDocument = PdfDocument.Load(new MemoryStream(fileBytes)))
            {
                foreach (int pageIndex in selectedPages)
                {
                    int actualPageIndex = pageIndex - 1;
                    Debug.WriteLine($"Processing page {actualPageIndex}...");

                    if (actualPageIndex >= 0 && actualPageIndex < pdfDocument.PageCount)
                    {
                        double inkCost = AnalyzeInkUsage(pdfDocument, actualPageIndex, colorStatus);
                        Debug.WriteLine($"Page {pageIndex}: Ink Cost = {inkCost}");
                        totalCost += inkCost;
                    }
                    else
                    {
                        Debug.WriteLine($"Error: Page {pageIndex} is out of range.");
                    }
                }
            }

            double finalTotal = Math.Ceiling(totalCost * copyCount);
            Debug.WriteLine($"Final Total Cost: {finalTotal}");
            return finalTotal;
        }


        private double AnalyzeInkUsage(PdfDocument pdfDocument, int pageIndex, string colorStatus)
        {
            if (pdfDocument == null)
            {
                Debug.WriteLine("Error: pdfDocument is null.");
                return 0.0;
            }

            using (SD.Image pageImage = pdfDocument.Render(pageIndex, 300, 300, true))
            {
                if (pageImage == null)
                {
                    Debug.WriteLine($"Error: Page {pageIndex} did not render properly.");
                    return 0.0;
                }

                using (SD.Bitmap bitmap = new SD.Bitmap(pageImage))
                {
                    int blackPixelCount = 0;
                    int colorPixelCount = 0;
                    int totalPixelCount = bitmap.Width * bitmap.Height;

                    Debug.WriteLine($"Page {pageIndex} - Width: {bitmap.Width}, Height: {bitmap.Height}, Total Pixels: {totalPixelCount}");
                    if (totalPixelCount == 0) return 0.0;

                    bool hasLargeImages = DetectLargeImages(bitmap);
                    BitmapData bitmapData = bitmap.LockBits(
                        new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb
                    );

                    IntPtr scan0 = bitmapData.Scan0;
                    int stride = bitmapData.Stride;
                    int bytesPerPixel = SD.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    byte[] pixelBuffer = new byte[stride * bitmap.Height];

                    Marshal.Copy(scan0, pixelBuffer, 0, pixelBuffer.Length);

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            int index = (y * stride) + (x * bytesPerPixel);
                            byte b = pixelBuffer[index];
                            byte g = pixelBuffer[index + 1];
                            byte r = pixelBuffer[index + 2];

                            bool isBlackPixel = (r < 50 && g < 50 && b < 50);
                            bool isColorPixel = (Math.Abs(r - g) > 20 || Math.Abs(r - b) > 20 || Math.Abs(g - b) > 20);

                            if (isBlackPixel) blackPixelCount++;
                            if (isColorPixel) colorPixelCount++;
                        }
                    }
                    bitmap.UnlockBits(bitmapData);

                    double blackInkRatio = (double)blackPixelCount / totalPixelCount;
                    double colorInkRatio = (double)colorPixelCount / totalPixelCount;
                    double finalCost = 0.0;

                    if (colorStatus == "greyscale")
                    {
                        if (blackInkRatio < 0.02)
                            finalCost = 3.0;
                        else if (blackInkRatio < 0.1)
                            finalCost = 3.0 + (blackInkRatio * 7.5);
                        else
                            finalCost = Math.Min(5.0, 4.5 + (blackInkRatio * 2.0));
                    }
                    else
                    {
                        if (colorInkRatio < 0.02)
                            finalCost = 4.0;
                        else if (colorInkRatio < 0.1)
                            finalCost = 5.0 + (colorInkRatio * 7.5);
                        else
                            finalCost = Math.Min(8.0, 6.5 + (colorInkRatio * 3.0));
                    }

                    Debug.WriteLine($"Page {pageIndex}: Black Ink Ratio = {blackInkRatio}, Color Ink Ratio = {colorInkRatio}, Final Cost = {finalCost}, Has Large Images = {hasLargeImages}");
                    return finalCost;
                }
            }
        }

        private bool DetectLargeImages(Bitmap bitmap)
        {
            int imageThreshold = (bitmap.Width * bitmap.Height) / 3;
            int nonWhitePixels = 0;
            int totalPixels = bitmap.Width * bitmap.Height;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    SD.Color pixel = bitmap.GetPixel(x, y);
                    bool isColorPixel = !(pixel.R == pixel.G && pixel.G == pixel.B);
                    bool isDarkPixel = (pixel.R < 180 && pixel.G < 180 && pixel.B < 180);
                    if (isColorPixel || isDarkPixel) nonWhitePixels++;
                }
            }
            return nonWhitePixels > imageThreshold;
        }

        private string NormalizePageSize(string pageSize)
        {
            return string.IsNullOrEmpty(pageSize) ? "default" : pageSize.Trim().ToLower();
        }


        private string NormalizeColorStatus(string colorStatus)
        {
            // Normalize: convert to lowercase and trim spaces
            return colorStatus?.Trim().ToLower();
        }


        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Calculate the total price
                double totalPrice = CalculateTotalPrice(FileBytes, ColorMode, SelectedPages, CopyCount);

                // Create an instance of YesOrNO modal window and pass parameters
                YesOrNO yesOrNoWindow = new YesOrNO(
                    FileBytes,    // File bytes
                    FileName,     // File name
                    PageSize,     // Page size
                    ColorMode,    // Color mode
                    SelectedPages.ToArray(), // Convert List<int> to int[]
                    CopyCount,    // Copy count
                    totalPrice    // Total price
                );

                // Open the YesOrNO window as a modal dialog
                yesOrNoWindow.Owner = Application.Current.MainWindow;
                yesOrNoWindow.ShowDialog(); // This will pause execution until the modal is closed
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the confirmation window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
