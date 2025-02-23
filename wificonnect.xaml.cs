using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    /// <summary>
    /// Interaction logic for wificonnect.xaml
    /// </summary>
    public partial class wificonnect : UserControl
    {
        public wificonnect()
        {
            InitializeComponent();
            DisplayHotspotQRCode();
        }

        private void DisplayHotspotQRCode()
        {
            string hotspotDetails = "WIFI:S:SnapPrintKiosk;T:WPA;P:123456789;;";
            Bitmap qrCodeImage = GenerateQRCode(hotspotDetails);
            QrCodeforhotspot.Source = BitmapToImageSource(qrCodeImage);
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Get reference to MainWindow
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Navigate to the default UserControl (e.g., HomeUserControl)
            mainWindow.MainContent.Content = new HomeUserControl(); // Replace with your actual default UserControl
        }

        private string GenerateSessionId(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Generate a session ID when the button is clicked
            string sessionId = GenerateSessionId();

            // Create an instance of the qrcode UserControl and pass the session ID
            qrcode qrPage = new qrcode(sessionId);

            // Access the MainWindow instance
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                // Set the content to display the QR page (assuming a ContentControl named MainContent)
                mainWindow.MainContent.Content = qrPage;
            }
            else
            {
                // Handle error if MainWindow is null
                MessageBox.Show("MainWindow instance is not available.");
            }
        }
    }
}
