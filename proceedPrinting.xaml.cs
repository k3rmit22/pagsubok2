using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    public partial class proceedPrinting : Window
    {
        private string FilePath { get; set; }
        private string FileName { get; set; }
        private string PaperSize { get; set; }
        private int PageCount { get; set; }
        private string ColorStatus { get; set; }

        // Update constructor to accept all necessary parameters
        public proceedPrinting(string filePath, string fileName, string paperSize, int pageCount, string colorStatus)
        {
            InitializeComponent();
            FilePath = filePath;
            FileName = fileName;
            PaperSize = paperSize;  // Assign the paper size
            PageCount = pageCount;  // Assign the page count
            ColorStatus = colorStatus;

            // Display the file details
            selectedFilePathTextBlock.Text = $"{FileName}";
            paperSizeTextBlock.Text = $"Paper Size: {PaperSize}";
            pageCountTextBlock.Text = $"Pages: {PageCount}";
            colorStatusTextBlock.Text = $"Color: {ColorStatus}";
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Limit the number of pages to 10
            if (PageCount > 10)
            {
                cautionTextBlock.Visibility = Visibility.Visible;
                cautionTextBlock.Text = "10 pages is the maximum page limit!";
                return;
            }

            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                var pdfDisplay = new PDFDisplay(FilePath, FileName, PaperSize, PageCount, ColorStatus);
                pdfDisplay.LoadPdfAsync(FilePath); // Load the PDF

                mainWindow.MainContent.Content = pdfDisplay; // Add PDFDisplay control to the main window
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}