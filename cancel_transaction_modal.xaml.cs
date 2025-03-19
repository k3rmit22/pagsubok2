using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for cancel_transaction_modal.xaml
    /// </summary>
    public partial class cancel_transaction_modal : Window
    {
       
        public string FileName { get; private set; }
        public string PageSize { get; private set; }
        public List<int> SelectedPages { get; private set; }
        public string ColorStatus { get; private set; }
        public int CopyCount { get; private set; }
     
        public double TotalPrice { get; private set; }

        public string Action { get; private set; } // New property

        public decimal TotalAmount { get; private set; } // Include TransactionData.TotalAmount


        public SerialPort SecondSerialPort { get; set; } // To send commands to COM9

        public cancel_transaction_modal(string fileName, string pageSize, 
                            string colorStatus, int copyCount,
                            List<int> selectedPages, double totalPrice)
        {

            InitializeComponent();
          
            FileName = fileName;
            PageSize = pageSize;
            ColorStatus = colorStatus;
            CopyCount = copyCount;
            SelectedPages = selectedPages;
            TotalPrice = totalPrice;
            Action = "Cancelled";
            TotalAmount = TransactionData.TotalAmount;

        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondSerialPort != null && SecondSerialPort.IsOpen)
            {
                try
                {
                    string cancelCommand = "servo180";
                    await Task.Run(() => SecondSerialPort.WriteLine(cancelCommand));
                    Debug.WriteLine($"Sent command to hardware via COM9: {cancelCommand}");
                }
                catch (Exception ex)
                {
                   
                    Debug.WriteLine($"Error sending command: {ex.StackTrace}");
                }
            }
            else
            {
                Debug.WriteLine("The second serial port is not open or available.", "Port Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Validate and send transaction data
            try
            {
                await SendTransactionAsync(
                    FileName,
                    PageSize,
                    SelectedPages,
                    ColorStatus,
                    CopyCount,
                    TotalPrice,
                    Action,
                    TotalAmount
                );

                Debug.WriteLine("Transaction data sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending transaction data: {ex.Message}");
            }

            // Navigate home
            NavigateToHomeUserControl();

            // Close the modal
            this.Close();
        }


        private async Task SendTransactionAsync(string fileName, string pageSize, List<int> selectedPages,
                                             string colorStatus, int copyCount, double totalPrice,
                                             string action, decimal totalAmount)
        {
            // Define your PHP API URL
            string url = "https://snapprintadmin.online/cancel_transaction.php"; // Replace with your actual API endpoint

            // Prepare the JSON payload using the class properties
            var transactionData = new
            {
                FileName = fileName,
                PageSize = pageSize,
                SelectedPages = selectedPages,
                ColorStatus = colorStatus,
                CopyCount = copyCount,
                Action = action,
                TotalPrice = totalPrice,
                TotalAmount = totalAmount  
            };

            // Serialize the object to JSON format
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(transactionData);

            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                try
                {
                    // Create HTTP content with the serialized JSON payload
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the POST request to the API endpoint
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Read the API response
                    string responseString = await response.Content.ReadAsStringAsync();

                    // (Optional) Log or display the response for debugging
                    Console.WriteLine($"API Response: {responseString}");
                }
                catch (Exception ex)
                {
                    // Handle errors during the API call
                    Console.WriteLine($"Error sending transaction data: {ex.Message}");
                }
            }
        }



        private void NavigateToHomeUserControl()
        {
            // Assuming the parent container is a Window (MainWindow) and it has a ContentControl or Frame for navigation
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // Assuming HomeUserControl is the UserControl you want to navigate to
                HomeUserControl homeControl = new HomeUserControl();

                // If using a ContentControl:
                mainWindow.MainContent.Content = homeControl;

                // If using a Frame:
                // mainWindow.MainFrame.Content = homeControl;
            }
        }

        private void NoButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Logic for "No" button, for example, just closing the modal without sending any command
            this.Close();
        }
    }
}
