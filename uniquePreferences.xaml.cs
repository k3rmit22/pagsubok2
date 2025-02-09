using Syncfusion.Windows.PdfViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace kiosk_snapprint
{
    public partial class uniquePreferences : UserControl
    {
        // Public properties to hold the values passed from PdfDisplayPage
        public string FileName { get; set; }
        public string PageSize { get; set; }
        public string ColorMode { get; set; }
        public List<int> SelectedPages { get; set; }

        private PricingQR _pricingQR;
        // PdfViewerControl to display the PDF
        private PdfViewerControl _pdfViewerControl;

        public PdfDisplayPage _previousPage;

        // Private field to hold the file bytes
        private byte[] _fileBytes;

        public uniquePreferences(PdfDisplayPage previousPage)
        {
            InitializeComponent();

            // Initialize the PdfViewerControl
            _pdfViewerControl = new PdfViewerControl();
            PdfContainer.Children.Add(_pdfViewerControl);  // Assuming you have a container named PdfContainer in XAML
            _previousPage = previousPage; // Store the previous page instance

            // Attach the Loaded event to trigger the color confirmation check
            this.Loaded += UniquePreferences_Loaded;
        }

        // Method to set preferences and display the PDF
        public void SetPreferences(string fileName, string pageSize, string colorMode, List<int> selectedPages, byte[] fileBytes)
        {
            Debug.WriteLine($"SetPreferences called with the following details:");
            Debug.WriteLine($"FileName: {fileName}");
            Debug.WriteLine($"PageSize: {pageSize}");
            Debug.WriteLine($"ColorMode: {colorMode}");
            Debug.WriteLine($"SelectedPages: {string.Join(", ", selectedPages)}");
            Debug.WriteLine($"FileBytes Length: {fileBytes?.Length ?? 0}");

            // Assign values to properties
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages;

            // Store the file bytes in the private field
            _fileBytes = fileBytes;

            // Display the PDF in PdfViewerControl
            if (_fileBytes != null && _fileBytes.Length > 0)
            {
                Debug.WriteLine("Loading PDF into PdfViewerControl...");
                MemoryStream pdfStream = new MemoryStream(_fileBytes);
                _pdfViewerControl.Load(pdfStream);
            }
            else
            {
                MessageBox.Show("Invalid PDF data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine("Error: Invalid PDF data.");
            }

            
        }

        // Method to access the stored file bytes
        public byte[] GetFileBytes()
        {
            return _fileBytes;
        }
        private void IncreaseCopyCount_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CopyCountTextBox.Text, out int currentCount) && currentCount < 10)
            {
                UpdateCopyCount(1);
            }
            else
            {
                MessageBox.Show("The maximum copy count is 10.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        // Decrease the copy count
        private void DecreaseCopyCount_Click(object sender, RoutedEventArgs e)
        {
            UpdateCopyCount(-1);
        }

        // Helper method to update the copy count
        private void UpdateCopyCount(int increment)
        {
            if (int.TryParse(CopyCountTextBox.Text, out int currentCount) && currentCount > 0)
            {
                currentCount += increment;
                if (currentCount < 1) currentCount = 1;  // Ensure the count is not less than 1
                Debug.WriteLine($"Updated copy count: {currentCount}");
                CopyCountTextBox.Text = currentCount.ToString();
            }
            else
            {
                Debug.WriteLine("Invalid copy count.");
            }
        }

        // Back button click handler
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to PdfDisplayPage
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = _previousPage;
        }


        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure that CopyCount is a valid number before passing it
            if (int.TryParse(CopyCountTextBox.Text, out int copyCount) && copyCount > 0)
            {
                // Create a new instance of Pricing_for_Unique
                Pricing_for_Unique pricingPage = new Pricing_for_Unique(
                    FileName,        // File name
                    PageSize,        // Page size
                    ColorMode,       // Color mode (colored or greyscale)
                    SelectedPages,   // List of selected pages
                    copyCount,       // Copy count (retrieved from CopyCountTextBox)
                    _fileBytes       // The byte array representing the file content
                );
                pricingPage._previousPage = this;
                // Navigate to the Pricing_for_Unique UserControl
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainContent.Content = pricingPage; // Assuming MainContent is the container for the content in the main window
            }
            else
            {
                // Show an error if the copy count is invalid or not set
                MessageBox.Show("Please enter a valid copy count.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void UniquePreferences_Loaded(object sender, RoutedEventArgs e)
        {
            if (ColorMode == "colored")
            {
                ConfirmColorMode();
            }
        }

        private void ConfirmColorMode()
        {
            // Create an instance of the modal
            ColorConfirmationModal modal = new ColorConfirmationModal();

            // Ensure the modal is shown in front of the main window
            modal.Owner = Application.Current.MainWindow;

            // Show the modal and wait for user input
            bool? result = modal.ShowDialog();

            if (result == true)
            {
                // Update ColorMode based on user's selection in the modal
                ColorMode = modal.SelectedColorStatus;
                System.Diagnostics.Debug.WriteLine($"User confirmed ColorMode: {ColorMode}");
            }
            else
            {
                // Handle when the modal is closed without selection
                System.Diagnostics.Debug.WriteLine("ColorConfirmationModal was closed without selection.");
            }
        }



    }
}