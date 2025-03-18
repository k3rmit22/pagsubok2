﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kiosk_snapprint
{
    public partial class uniquecode : UserControl
    {
        public uniquecode()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = new HomeUserControl();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Enable PROCEED button only if text is entered in the TextBox
            Proceed.IsEnabled = !string.IsNullOrWhiteSpace(TxtBox.Text);
        }

        private async void Proceed_Click(object sender, RoutedEventArgs e)
        {
            string uniqueCode = TxtBox.Text.Trim();

            if (string.IsNullOrEmpty(uniqueCode))
            {
                MessageBox.Show("Please enter the unique code.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ✅ Show the loading circle immediately
                LoadingPanel.Visibility = Visibility.Visible;
                await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

                // ✅ Set a timeout for 20 seconds
                var cancellationTokenSource = new CancellationTokenSource();
                var timeoutTask = Task.Delay(20000, cancellationTokenSource.Token);  // 20 seconds timeout

                // ✅ Start the file retrieval task
                var retrievalTask = RetrieveFileAsync(uniqueCode);

                // ✅ Wait for either the retrieval or timeout
                var completedTask = await Task.WhenAny(retrievalTask, timeoutTask);

                // ✅ Hide the loading circle immediately after retrieval or timeout
                LoadingPanel.Visibility = Visibility.Collapsed;

                if (completedTask == timeoutTask)
                {
                    // 🛑 Timeout occurred
                    MessageBox.Show("An error has occurred. The process took too long.", "Timeout Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ✅ Retrieve the result
                (byte[] fileBytes, string fileName) = await retrievalTask;

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    MessageBox.Show("The file is empty or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ✅ Navigate to the PDF display page
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                var pdfDisplayPage = new PdfDisplayPage(fileBytes, fileName);
                mainWindow.MainContent.Content = pdfDisplayPage;
            }
            catch (Exception ex)
            {
                // ✅ Hide the loading circle in case of an exception
                LoadingPanel.Visibility = Visibility.Collapsed;

                MessageBox.Show($"An error occurred while retrieving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<(byte[] fileBytes, string fileName)> RetrieveFileAsync(string uniqueCode)
        {
            try
            {
                string apiUrl = $"https://snapprintt-hkbsdyarheeedjc2.canadacentral-01.azurewebsites.net/api/Upload/GetFile/{uniqueCode}";
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                        string fileName = response.Content.Headers.ContentDisposition?.FileName ?? "UnknownFile.pdf";
                        return (fileBytes, fileName);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No file found for the given unique code.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return (null, null);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error retrieving file: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return (null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Network error: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }
    }
}
