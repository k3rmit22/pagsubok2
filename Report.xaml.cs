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

        // Connection string for your MySQL database
        string connectionString = "Server=localhost; Database=admin_user; Uid=root; Pwd=;";

        // Helper method to insert the report into the database
        private void InsertReport(string issue)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO tbl_reports (issue, datetime, status) VALUES (@issue, @datetime, @status)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@issue", issue);
                        cmd.Parameters.AddWithValue("@datetime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@status", "Pending");

                        cmd.ExecuteNonQuery();
                    }
                }

                // Show success message
                MessageBox.Show("Report submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigate back to HomeUserControl
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainContent.Content = new HomeUserControl();
            }
            catch (Exception ex)
            {
                // Show error message if insertion fails
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for "Printer Malfunction" button
        private void ReportPrinterMalfunction(object sender, RoutedEventArgs e)
        {
            InsertReport("Printer Malfunction");
        }

        // Event handler for "Paper Jam" button
        private void ReportPaperJam(object sender, RoutedEventArgs e)
        {
            InsertReport("Paper Jam");
        }

        // Event handler for "Payment Issues" button
        private void ReportPaymentIssues(object sender, RoutedEventArgs e)
        {
            InsertReport("Payment Issues");
        }

        // Event handler for "Paper Out of Stock" button
        private void ReportPaperOutOfStock(object sender, RoutedEventArgs e)
        {
            InsertReport("Paper Out of Stock");
        }

        // Event handler for "Back" button
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainContent.Content = new HomeUserControl();
        }
    }
}
