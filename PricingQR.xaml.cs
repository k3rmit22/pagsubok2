using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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

        private readonly Dictionary<string, int> ColorStatusValues = new Dictionary<string, int>
        {
            { "colored", 10 },
            { "greyscale", 5 }
        };

        private readonly Dictionary<string, int> PageSizeValues = new Dictionary<string, int>
        {
            { "a4", 5 },
            { "letter (short)", 5 },
            { "legal (long)", 10 }
        };

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
            LoadSummary(FileName, PageSize, ColorStatus, SelectedPages, CopyCount);
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName; // Update the FileName property
            if (filename != null)
            {
                filename.Text = fileName; // Update the UI element if applicable
            }
        }

        private void LoadSummary(string fileName, string pageSize, string colorStatus, List<int> selectedPages, int copyCount)
        {
            // Normalize input values (automatically convert to lowercase and trim spaces)
            string normalizedPageSize = NormalizePageSize(pageSize);
            string normalizedColorStatus = NormalizeColorStatus(colorStatus);

            // Display data
            filename.Text = fileName;
            color_label.Text = normalizedColorStatus;
            pagesize_label.Text = normalizedPageSize;
            Copies_label.Text = copyCount.ToString();

            if (selected_pages_label != null && selectedPages != null)
            {
                // Display the selected pages as a comma-separated string
                selected_pages_label.Text = string.Join(", ", selectedPages);
            }

            // Compute the total price using your logic and set the TotalPrice property
            double totalPrice = ComputeTotalPrice(normalizedColorStatus, selectedPages.Count, copyCount);
            TotalPrice = totalPrice;  // Set TotalPrice property

            // Display the total price
            total_label.Text = $"{totalPrice}";
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

        private double ComputeTotalPrice(string colorStatus, int numberOfSelectedPages, int copyCount)
        {
            // Log the incoming color status for debugging purposes
            System.Diagnostics.Debug.WriteLine($"Computing total price for color status: {colorStatus}");

            // Check if the colorStatus exists in the dictionary
            if (ColorStatusValues.TryGetValue(colorStatus, out int colorValue))
            {
                // Formula: Color Value * Number of Selected Pages * Copy Count
                double totalPrice = colorValue * numberOfSelectedPages * copyCount;

                // Log the computed price for debugging purposes
                System.Diagnostics.Debug.WriteLine($"Total Price: {totalPrice}");

                return totalPrice;
            }
            else
            {
                // Log an error or warning if the colorStatus is not found
                System.Diagnostics.Debug.WriteLine($"Invalid color status: {colorStatus}. Returning 0.0 as default.");

                // Return 0.0 or handle a default value if the color status is invalid
                return 0.0;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the insert_payment UserControl and pass the data, including the totalPrice
            insert_payment insertPaymentControl = new insert_payment(
                filePath: FilePath,
                fileName: FileName,
                pageSize: PageSize,
                pageCount: Pagecount,
                colorStatus: ColorStatus,
                numberOfSelectedPages: NumberOfSelectedPages,
                copyCount: CopyCount,
                selectedPages: SelectedPages,
                totalPrice: TotalPrice // Pass the totalPrice to insert_payment
            );

            // Access the MainWindow instance
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                // Set the content to display the insert_payment page
                mainWindow.MainContent.Content = insertPaymentControl;
            }
            else
            {
                // Handle error if MainWindow is null
                MessageBox.Show("MainWindow instance is not available.");
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