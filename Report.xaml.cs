using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



namespace kiosk_snapprint
{
    public partial class Report : UserControl
    {
        public Report()
        {
            InitializeComponent();
        }

       
        // Event handler for "Back" button
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = new HomeUserControl();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await SendDataToApiAsync("refund hardware issue");
                MessageBox.Show("Data successfully sent!");
                NavigateToMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send data: {ex.Message}");
            }



        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {

                await SendDataToApiAsync("printing issue");
                MessageBox.Show("Data successfully sent!");
                NavigateToMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send data: {ex.Message}");
            }


        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {

                await SendDataToApiAsync("hardware issue");
                MessageBox.Show("Data successfully sent!");
                NavigateToMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send data: {ex.Message}");
            }



        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {

                await SendDataToApiAsync("System issue");
            MessageBox.Show("Data successfully sent!");
            NavigateToMainWindow();
              }
                 catch (Exception ex)
                  {
        MessageBox.Show($"Failed to send data: {ex.Message}");
            }


        }

// Method to send data to the PHP API
 private async Task SendDataToApiAsync(string issue)
        {
            using (var client = new HttpClient())
            {
                // Set the API endpoint (replace with your actual URL)
                string apiUrl = "https://snapprintadmin.online/reportapi.php";

                // Prepare the JSON payload
                var data = new
                {
                    issue = issue,
                    datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // Current timestamp
                };

                // Serialize the data to JSON
                var jsonContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);

                // Check the response
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Success: {responseString}");
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode}");
                }
            }
        }

        private void NavigateToMainWindow()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = new HomeUserControl();
        }


    }
}
