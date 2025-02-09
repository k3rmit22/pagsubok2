using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRCoder;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using static kiosk_snapprint.qrcode;

namespace kiosk_snapprint
{
    public partial class qrcode : UserControl
    {
        private string sessionId;
        private HubConnection hubConnection;

        public qrcode(string sessionId)
        {
            InitializeComponent();
            this.sessionId = sessionId;
            
            DisplayQRCode();
            SessionIdTextBlock.Text = $"Session ID: {sessionId}";

            SetupSignalR();    // Initialize SignalR connection
        }

        


        private void DisplayQRCode()
        {
           
            string url = $"http://192.168.137.1:5082/Upload/Index?sessionId={sessionId}";
            Console.WriteLine($"Generated URL: {url}");
            Bitmap qrCodeImage = GenerateQRCode(url);
            QrCodeImageControl.Source = BitmapToImageSource(qrCodeImage);

        }


        private async void SetupSignalR()
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.137.1:5082/Hubs/fileUploadHub")
                .Build();

            // Register a method to receive messages from the server
            hubConnection.On<string>("ReceiveMessage", async (message) =>
            {
                System.Diagnostics.Debug.WriteLine($"Received SignalR message: {message}");
                if (message.Contains($"File uploaded successfully for session {sessionId}"))
                {
                    // Run the file fetching in the background without blocking the UI thread
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await FetchFileDetails();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error fetching file details: {ex.Message}");
                        }
                    });
                }
            });

            // Start the SignalR connection
            try
            {
                await hubConnection.StartAsync();
                System.Diagnostics.Debug.WriteLine("Connected to SignalR hub.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
            }
        }



        private async Task FetchFileDetails()
        {
            try
            {
                // Run the HTTP request on a background thread to avoid blocking the UI thread
                var fileDetails = await Task.Run(async () =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string apiUrl = $"http://192.168.137.1:5082/api/upload/getfileinfo?sessionId={sessionId}";

                        // Send request to retrieve file details
                        var response = await client.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            // Deserialize file details
                            return await response.Content.ReadFromJsonAsync<FileDetails>();
                        }
                        else
                        {
                            ShowError("Unable to retrieve file details.");
                            return null;
                        }
                    }
                });

                // After getting file details, we need to interact with UI components, which must be done on the UI thread
                if (fileDetails != null && fileDetails.SessionId == sessionId)
                {
                    // Ensure UI updates are done on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Display file details in the console
                        System.Diagnostics.Debug.WriteLine($"Session ID: {fileDetails.SessionId}");
                        System.Diagnostics.Debug.WriteLine($"File Path: {fileDetails.FilePath}");
                        System.Diagnostics.Debug.WriteLine($"File Name: {fileDetails.FileName}");
                        System.Diagnostics.Debug.WriteLine($"Page Size: {fileDetails.PageSize}");
                        System.Diagnostics.Debug.WriteLine($"Page color: {fileDetails.ColorStatus}");


                        NavigateToPDFDisplay(fileDetails);
                    });
                }
                else
                {
                    ShowError("Session ID mismatch.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error fetching file details: " + ex.Message);
                Console.WriteLine($"Error fetching file details: {ex.Message}");
            }
        }



        private Bitmap GenerateQRCode(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(10); // Size customization
        }

        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

     
        private async void NavigateToPDFDisplay(FileDetails fileDetails)
        {
            // Create an instance of PDFDisplay and pass the file details
            var pdfDisplay = new PDFDisplay(fileDetails.FilePath, fileDetails.FileName, fileDetails.PageSize, fileDetails.PageCount, fileDetails.ColorStatus);

            // Retrieve the current main window instance and set the content on the UI thread
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Ensure this happens on the UI thread
                await mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.MainContent.Content = pdfDisplay; // Update MainContent in the MainWindow instance
                });

                // Once the content is set, load the PDF asynchronously
                await pdfDisplay.LoadPdfAsync(fileDetails.FilePath);
            }
            else
            {
                ShowError("Main window instance not found.");
            }
        }




        private void ShowError(string message)
        {
            //MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
        }

        public class FileDetails
        {
            public string SessionId { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string PageSize { get; set; }
            public int PageCount { get; set; }
            public string ColorStatus { get; set; }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Get reference to MainWindow
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Navigate to the default UserControl (e.g., HomeUserControl)
            mainWindow.MainContent.Content = new wificonnect(); // Replace with your actual default UserControl
        }
    }

   
}
