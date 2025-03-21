using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
