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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl : UserControl
    {
        public HomeUserControl()
        {
            InitializeComponent();
            TransactionData.Reset();

        }

        private void GoToQRCodePage_Click(object sender, RoutedEventArgs e)
        {
            // Generate a session ID when the button is clicked
            wificonnect connect2Wifi = new wificonnect();
            MainContent.Content = connect2Wifi;
        }
       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            flashdrive flashdrivePage = new flashdrive();
            MainContent.Content = flashdrivePage;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            uniquecode uniquecodePage = new uniquecode();
            MainContent.Content = uniquecodePage;

        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            Report ReportPage = new Report();
            MainContent.Content = ReportPage;

        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            paper_refill maintenance = new paper_refill();
            MainContent.Content = maintenance;
        }
    }
}
