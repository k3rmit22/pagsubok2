using System;
using System.Collections.Generic;
using System.Windows;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for ProceedToPayment.xaml
    /// </summary>
    public partial class ProceedToPayment : Window
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

        public ProceedToPayment(string filePath, string fileName, string pageSize, int pageCount,
                                string colorStatus, int numberOfSelectedPages, int copyCount,
                                List<int> selectedPages, double totalPrice)
        {
            InitializeComponent();

            // Store passed values
            FilePath = filePath;
            FileName = fileName;
            PageSize = pageSize;
            PageCount = pageCount; // Fixed variable name
            ColorStatus = colorStatus;
            NumberOfSelectedPages = numberOfSelectedPages;
            CopyCount = copyCount;
            SelectedPages = selectedPages ?? new List<int>(); // Ensure it's not null
            TotalPrice = totalPrice;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the insert_payment UserControl and pass the data, including the totalPrice
            insert_payment insertPaymentControl = new insert_payment(
                filePath: FilePath,
                fileName: FileName,
                pageSize: PageSize,
                pageCount: PageCount, // Fixed typo
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
