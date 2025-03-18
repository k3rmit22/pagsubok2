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
    public partial class PricingQR : UserControl
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string PageSize { get; private set; }
        public int Pagecount { get; private set; }
        public string ColorStatus { get; private set; }
        public int NumberOfSelectedPages { get; private set; }
        public int CopyCount { get; private set; }
        public List<int> SelectedPages { get; private set; }

        public double TotalPrice { get; set; }

      

        public PricingQR(string filePath, string fileName, string pageSize, string colorStatus,
            int numberOfSelectedPages, int copyCount, List<int> selectedPages, int pageCount)
        {
            InitializeComponent();

            // Store data in properties
            FilePath = filePath;
            FileName = fileName;
            PageSize = pageSize;
            Pagecount = pageCount;
            ColorStatus = colorStatus;
            NumberOfSelectedPages = numberOfSelectedPages;
            CopyCount = copyCount;
            SelectedPages = selectedPages;

            // Load summary and display data
            LoadSummary(FilePath,FileName, PageSize, ColorStatus, SelectedPages, CopyCount);
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName; // Update the FileName property
            if (filename != null)
            {
                filename.Text = fileName; // Update the UI element if applicable
            }
        }

        private void LoadSummary(string filepath,string fileName, string pageSize, string colorStatus, List<int> selectedPages, int copyCount)
        {
            
            System.Diagnostics.Debug.WriteLine("LoadSummary CALLED!");
            string normalizedPageSize = NormalizePageSize(pageSize);
            string normalizedColorStatus = NormalizeColorStatus(colorStatus);

            filename.Text = fileName;
            color_label.Text = normalizedColorStatus;
            pagesize_label.Text = normalizedPageSize;
            Copies_label.Text = copyCount.ToString();

            if (selected_pages_label != null && selectedPages != null)
            {
                selected_pages_label.Text = string.Join(", ", selectedPages);
            }

            // ✅ Fix: Pass filePath correctly
            double totalPrice = ComputeTotalPrice(filepath, normalizedColorStatus, selectedPages, copyCount);
            TotalPrice = totalPrice;

            total_label.Text = $"{totalPrice}";
        }


        private double ComputeTotalPrice(string filePath, string colorStatus, List<int> selectedPages, int copyCount)
        {
            System.Diagnostics.Debug.WriteLine($"ComputeTotalPrice CALLED! FilePath: {filePath}, ColorStatus: {colorStatus}, Copies: {copyCount}");

            if (selectedPages == null || selectedPages.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Error: No pages selected for printing.");
                return 0.0;
            }

            double totalCost = 0.0;
            System.Diagnostics.Debug.WriteLine($"Opening PDF file: {filePath}");

            using (var pdfDocument = PdfDocument.Load(filePath))
            {
                foreach (int pageIndex in selectedPages)
                {
                    int actualPageIndex = pageIndex - 1; // Convert 1-based index to 0-based

                    System.Diagnostics.Debug.WriteLine($"Processing page {actualPageIndex}...");

                    if (actualPageIndex >= 0 && actualPageIndex < pdfDocument.PageCount)
                    {
                        double inkCost = AnalyzeInkUsage(pdfDocument, actualPageIndex, colorStatus);
                        System.Diagnostics.Debug.WriteLine($"Page {pageIndex}: Ink Cost = {inkCost}");

                        totalCost += inkCost;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: Page {pageIndex} is out of range.");
                    }
                }
            }

            double finalTotal = Math.Ceiling(totalCost * copyCount);
            System.Diagnostics.Debug.WriteLine($"Final Total Cost: {finalTotal}");
            return finalTotal;
        }



        private double AnalyzeInkUsage(PdfDocument pdfDocument, int pageIndex, string colorStatus)
        {
            if (pdfDocument == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: pdfDocument is null.");
                return 0.0;
            }

            using (SD.Image pageImage = pdfDocument.Render(pageIndex, 300, 300, true))
            {
                if (pageImage == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: Page {pageIndex} did not render properly.");
                    return 0.0;
                }

                using (SD.Bitmap bitmap = new SD.Bitmap(pageImage))
                {
                    int blackPixelCount = 0;
                    int colorPixelCount = 0;
                    int totalPixelCount = bitmap.Width * bitmap.Height;

                    System.Diagnostics.Debug.WriteLine($"Page {pageIndex} - Width: {bitmap.Width}, Height: {bitmap.Height}, Total Pixels: {totalPixelCount}");

                    if (totalPixelCount == 0) return 0.0;

                    // Detect if the page is image-heavy
                    bool hasLargeImages = DetectLargeImages(bitmap);

                    // Lock the bitmap for faster pixel processing
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

                    // Process pixels
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            int index = (y * stride) + (x * bytesPerPixel);
                            byte b = pixelBuffer[index];
                            byte g = pixelBuffer[index + 1];
                            byte r = pixelBuffer[index + 2];

                            bool isBlackPixel = (r < 50 && g < 50 && b < 50); // Dark pixel (black ink)
                            bool isColorPixel = (Math.Abs(r - g) > 20 || Math.Abs(r - b) > 20 || Math.Abs(g - b) > 20); // Detect color variation

                            if (isBlackPixel)
                                blackPixelCount++;
                            if (isColorPixel)
                                colorPixelCount++;
                        }
                    }

                    bitmap.UnlockBits(bitmapData);

                    double blackInkRatio = (double)blackPixelCount / totalPixelCount;
                    double colorInkRatio = (double)colorPixelCount / totalPixelCount;

                    // Determine the final cost based on ink usage and page type
                    double finalCost = 0.0;

                    if (colorStatus == "greyscale")
                    {
                        if (blackInkRatio < 0.02) // Mostly text-only
                        {
                            finalCost = 3.0;
                        }
                        else if (blackInkRatio < 0.1) // Small images
                        {
                            finalCost = 3.0 + (blackInkRatio * 7.5); // Adjusts within ₱3-₱4.5
                        }
                        else // Image-heavy
                        {
                            finalCost = Math.Min(5.0, 4.5 + (blackInkRatio * 2.0)); // ₱4.5 - ₱5
                        }
                    }
                    else // Colored pages
                    {
                        if (colorInkRatio < 0.02) // Mostly text-only with small highlights
                        {
                            finalCost = 4.0; // Lowered price for low-color text pages
                        }
                        else if (colorInkRatio < 0.1) // Medium image usage
                        {
                            finalCost = 5.0 + (colorInkRatio * 7.5); // Adjusts within ₱4 - ₱6
                        }
                        else // High color usage
                        {
                            finalCost = Math.Min(8.0, 6.5 + (colorInkRatio * 3.0)); // ₱6.5 - ₱8
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Page {pageIndex}: Black Ink Ratio = {blackInkRatio}, Color Ink Ratio = {colorInkRatio}, Final Cost = {finalCost}, Has Large Images = {hasLargeImages}");
                    return finalCost;
                }
            }
        }


        private bool DetectLargeImages(Bitmap bitmap)
        {
            int imageThreshold = (bitmap.Width * bitmap.Height) / 3;  // 33% threshold
            int nonWhitePixels = 0;
            int totalPixels = bitmap.Width * bitmap.Height;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    SD.Color pixel = bitmap.GetPixel(x, y);
                    bool isColorPixel = !(pixel.R == pixel.G && pixel.G == pixel.B); // Color pixel check
                    bool isDarkPixel = (pixel.R < 180 && pixel.G < 180 && pixel.B < 180);

                    if (isColorPixel || isDarkPixel)
                    {
                        nonWhitePixels++;
                    }
                }
            }

            return nonWhitePixels > imageThreshold;
        }














        private string NormalizePageSize(string pageSize)
        {
            // Normalize: convert to lowercase and trim spaces
            return pageSize?.Trim().ToLower();
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
                // Create an instance of the ProceedToPayment window and pass the necessary parameters
                ProceedToPayment proceedToPaymentWindow = new ProceedToPayment(
                    filePath: FilePath,
                    fileName: FileName,
                    pageSize: PageSize,
                    pageCount: Pagecount,
                    colorStatus: ColorStatus,
                    numberOfSelectedPages: NumberOfSelectedPages,
                    copyCount: CopyCount,
                    selectedPages: SelectedPages,
                    totalPrice: TotalPrice
                );

                // Set the owner to the main window
                proceedToPaymentWindow.Owner = Application.Current.MainWindow;

                // Open the ProceedToPayment window as a modal dialog
                proceedToPaymentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the confirmation window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Access the MainWindow instance
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                // Create a new instance of PricingQR with the current properties
                QR_preferences preferences = new QR_preferences(
                    filePath: FilePath,
                    fileName: FileName,
                    pageSize: PageSize,
                    colorStatus: ColorStatus,
                    numberOfSelectedPages: NumberOfSelectedPages,
                    copyCount: CopyCount,
                    selectedPages: SelectedPages,
                    pageCount: Pagecount
                );

                // Set the content to the PricingQR UserControl
                mainWindow.MainContent.Content = preferences;
            }
            else
            {
                // Handle error if MainWindow is null
                MessageBox.Show("MainWindow instance is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}