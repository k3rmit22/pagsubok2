using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for YesOrNO.xaml
    /// </summary>
    public partial class YesOrNO : Window
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

        public YesOrNO(byte[] fileBytes, string fileName, string pageSize, string colorMode, int[] selectedPages, int copyCount, double totalPrice)
        {
            InitializeComponent();

            // Store the received parameters
            FileBytes = fileBytes;
            FileName = fileName;
            PageSize = pageSize;
            ColorMode = colorMode;
            SelectedPages = selectedPages.ToList(); // Convert int[] to List<int>
            CopyCount = copyCount;
            TotalPrice = totalPrice;
        }


        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of Uniquecode_insert_payment and pass the parameters
            Uniquecode_insert_payment paymentPage = new Uniquecode_insert_payment(
                FileBytes,
                FileName,
                PageSize,
                ColorMode,
                SelectedPages,
                CopyCount,
                TotalPrice
            );

            // Navigate to the next user control
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = paymentPage; // Assuming MainContent is the container

            // Close the YesOrNO window
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the YesOrNO window without proceeding
            this.Close();
        }
    }


}
