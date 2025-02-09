using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Syncfusion.Windows.PdfViewer;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;


namespace kiosk_snapprint
{

    public partial class PDFDisplay : UserControl
    {

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string PageSize { get; private set; }
        public int PageCount { get; private set; }

        public string Colorstatus { get; private set; }

        private List<int> selectedPages = new List<int>();

        public PDFDisplay(string filePath, string fileName, string pageSize, int pageCount, string colorstatus)
        {
            InitializeComponent();


            // Store the file details
            FilePath = filePath;
            FileName = fileName;
            PageSize = pageSize;
            PageCount = pageCount;
            Colorstatus = colorstatus;

            // Initialize the copy count display
            pdfViewer.ZoomMode = Syncfusion.Windows.PdfViewer.ZoomMode.FitPage;

            PopulatePageCheckboxes(filePath);
        }

        public void UpdatePdfDetails(string filePath, string fileName, string pageSize, int pageCount, string colorStatus)
        {
            FilePath = filePath;
            FileName = fileName;
            PageSize = pageSize;
            PageCount = pageCount;
            Colorstatus = colorStatus;

            // Reload the PDF with the updated details
            EnsurePdfLoaded();
        }

        private void EnsurePdfLoaded()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                try
                {
                    pdfViewer.Load(FilePath); // Load the PDF synchronously
                }
                catch (Exception ex)
                {
                    ShowError($"Error ensuring PDF loaded: {ex.Message}");
                }
            }
        }

        public async Task LoadPdfAsync(string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Ensure pdfViewer is accessed on the UI thread
                    Dispatcher.Invoke(() =>
                    {
                        if (pdfViewer != null)
                        {
                            try
                            {
                                pdfViewer.Load(filePath); // Load the PDF using Syncfusion PdfViewer's Load method
                            }
                            catch (Exception ex)
                            {
                                ShowError($"Error loading PDF: {ex.Message}");
                            }
                        }
                        else
                        {
                            ShowError("PDF Viewer instance is not initialized.");
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                ShowError($"Error loading PDF: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            // Display an error message only if the PDF failed to load
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }



        // Method to populate checkboxes for each page in the PDF
        private void PopulatePageCheckboxes(string filePath)
        {
            try
            {
                // Clear the StackPanel to avoid duplicates if called multiple times
                PageSelectionStackPanel.Children.Clear();

                // Create "Select All Pages" checkbox
                CheckBox selectAllCheckBox = new CheckBox
                {
                    Content = "Select All Pages",
                    FontSize = 15,
                    Tag = "SelectAll" // Tag to distinguish it from page checkboxes
                };

                // Add event handlers for the "Select All Pages" checkbox
                selectAllCheckBox.Checked += SelectAllCheckBox_Checked;
                selectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;

                // Add the "Select All Pages" checkbox to the StackPanel
                PageSelectionStackPanel.Children.Add(selectAllCheckBox);

                // Open the PDF to get the page count using iText7
                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    PageCount = pdfDoc.GetNumberOfPages(); // Get the total page count
                    for (int i = 1; i <= PageCount; i++)
                    {
                        // Create a CheckBox for each page
                        CheckBox pageCheckBox = new CheckBox
                        {
                            Content = $"Page {i}",
                            FontSize = 15,
                            Tag = i // Store the page number in the Tag for later retrieval
                        };

                        // Add event handlers for checking/unchecking individual checkboxes
                        pageCheckBox.Checked += PageCheckBox_Checked;
                        pageCheckBox.Unchecked += PageCheckBox_Unchecked;

                        // Add the CheckBox to the StackPanel
                        PageSelectionStackPanel.Children.Add(pageCheckBox);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error reading PDF: {ex.Message}");
            }
        }

        // Event handler for "Select All Pages" checkbox checked
        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var child in PageSelectionStackPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox.Tag is int) // Skip the "Select All" checkbox
                {
                    checkBox.IsChecked = true; // Select all individual page checkboxes
                }
            }
        }

        // Event handler for "Select All Pages" checkbox unchecked
        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var child in PageSelectionStackPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox.Tag is int) // Skip the "Select All" checkbox
                {
                    checkBox.IsChecked = false; // Deselect all individual page checkboxes
                }
            }
        }

        // Event handler for when a page checkbox is checked
        private void PageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag is int pageNumber)
            {
                if (!selectedPages.Contains(pageNumber)) // Prevent duplicate entries
                {
                    selectedPages.Add(pageNumber);
                }

                // Check if all individual checkboxes are checked to update "Select All"
                UpdateSelectAllCheckboxState();
            }
        }

        // Event handler for when a page checkbox is unchecked
        private void PageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag is int pageNumber)
            {
                selectedPages.Remove(pageNumber);
            }

            // Check if all individual checkboxes are checked to update "Select All"
            UpdateSelectAllCheckboxState();
        }

        // Method to update the "Select All Pages" checkbox state
        private void UpdateSelectAllCheckboxState()
        {
            bool allChecked = true;

            foreach (var child in PageSelectionStackPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox.Tag is int) // Skip the "Select All" checkbox
                {
                    if (checkBox.IsChecked != true)
                    {
                        allChecked = false;
                        break;
                    }
                }
            }

            // Update the "Select All" checkbox state
            var selectAllCheckBox = PageSelectionStackPanel.Children[0] as CheckBox;
            if (selectAllCheckBox != null)
            {
                selectAllCheckBox.Checked -= SelectAllCheckBox_Checked; // Temporarily remove the event handler
                selectAllCheckBox.Unchecked -= SelectAllCheckBox_Unchecked;
                selectAllCheckBox.IsChecked = allChecked;
                selectAllCheckBox.Checked += SelectAllCheckBox_Checked; // Reattach the event handler
                selectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;
            }
        }


        private void PROCEED_Click(object sender, RoutedEventArgs e)
        {
            // Ensure the PDF is loaded before proceeding
            if (pdfViewer.DocumentInfo != null)
            {
                // Validation: Ensure at least one page is selected
                if (selectedPages.Count == 0)
                {
                    ShowError("Please select at least one page before proceeding.");
                    return;
                }

                // Proceed with QR preferences after validation
                QR_preferences qrPreferences = new QR_preferences(FilePath, FileName, PageSize, PageCount, Colorstatus, selectedPages, selectedPages.Count);

                // Set the MainContent in MainWindow
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainContent.Content = qrPreferences;
                }
                else
                {
                    ShowError("MainWindow instance is not available.");
                }
            }
            else
            {
                // Handle PDF not loaded scenario if needed
                ShowError("PDF is not loaded properly.");
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Get reference to MainWindow
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Navigate to the default UserControl (e.g., HomeUserControl)
            mainWindow.MainContent.Content = new HomeUserControl(); // Replace with your actual default UserControl
        }
    }

}