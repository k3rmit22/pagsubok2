using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            // Set the UI elements with the values passed in the constructor
            filename.Text = FileName; // Display the file name
            color_label.Text = ColorMode; // Display the color mode
            pagesize_label.Text = PageSize;
            selected_pages_label.Text = string.Join(", ", SelectedPages); // Display the selected pages as a comma-separated list
            Copies_label.Text = CopyCount.ToString(); // Display the copy count

            try
            {
                // Get the numeric value for ColorMode
                int colorModeValue = GetColorModeValue(ColorMode);

                // Use colorModeValue in calculations
                int totalPrice = CalculateTotalPrice(ColorMode, SelectedPages, CopyCount);

                // Display the calculated total price (ensure TotalPriceLabel exists in the UI)
                total_label.Text = totalPrice.ToString();

                Console.WriteLine($"Color Mode: {ColorMode}, Numeric Value: {colorModeValue}, Total Price: {totalPrice}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                color_label.Text = "Invalid Color Mode"; // Display an error message in the UI
            }
        }
        public int GetColorModeValue(string colorMode)
        {
            // Normalize the input to prevent case sensitivity issues
            if (colorMode.Equals("colored", StringComparison.OrdinalIgnoreCase))
            {
                return 10; // Return 10 for colored
            }
            else if (colorMode.Equals("greyscale", StringComparison.OrdinalIgnoreCase))
            {
                return 5; // Return 5 for greyscale
            }
            else
            {
                throw new ArgumentException("Invalid color mode value."); // Handle unexpected input
            }
        }

        public int CalculateTotalPrice(string colorMode, List<int> selectedPages, int copyCount)
        {
            // Determine the numeric value based on the ColorMode string
            int colorModeValue = 0;

            if (colorMode.Equals("colored", StringComparison.OrdinalIgnoreCase))
            {
                colorModeValue = 10; // Colored pages have a value of 10
            }
            else if (colorMode.Equals("greyscale", StringComparison.OrdinalIgnoreCase))
            {
                colorModeValue = 5;  // Greyscale pages have a value of 5
            }
            else
            {
                // Handle invalid color mode (optional)
                throw new ArgumentException("Invalid color mode.");
            }

            // Get the count of selected pages
            int pageCount = selectedPages.Count;

            // Calculate total price
            return colorModeValue * pageCount * copyCount;
        }


        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Calculate the total price without converting color mode
                int totalPrice = CalculateTotalPrice(ColorMode, SelectedPages, CopyCount); // Pass ColorMode directly

                // Create an instance of the next user control (FinalReview)
                Uniquecode_insert_payment paymentPage = new Uniquecode_insert_payment(
                    FileBytes,    // File bytes passed from Pricing_for_Unique
                    FileName,     // File name passed from Pricing_for_Unique
                    PageSize,     // Page size passed from Pricing_for_Unique
                    ColorMode,    // Color mode passed directly (no conversion)
                    SelectedPages,// Selected pages passed from Pricing_for_Unique
                    CopyCount,    // Copy count passed from Pricing_for_Unique
                    totalPrice    // Total price calculated
                );

                // Navigate to the next user control (FinalReview)
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainContent.Content = paymentPage; // Assuming MainContent is the container for the user controls
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while navigating to the next page: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
