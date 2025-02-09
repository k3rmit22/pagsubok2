using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for flashdrive.xaml
    /// </summary>
    public partial class flashdrive : UserControl
    {
        private DispatcherTimer driveDetectionTimer;

        public flashdrive()
        {
            InitializeComponent();
            InitializeDriveDetection();
        }

        private void InitializeDriveDetection()
        {
            // Create a timer to periodically check for flash drive insertion
            driveDetectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Check every 2 seconds (can be adjusted)
            };
            driveDetectionTimer.Tick += DriveDetectionTimer_Tick;
            driveDetectionTimer.Start();
        }

        private void DriveDetectionTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Get all drives and check for removable devices
                var drives = DriveInfo.GetDrives()
                                       .Where(d => d.IsReady && d.DriveType == DriveType.Removable)
                                       .ToList();

                if (drives.Any())
                {
                    // Flash drive is detected, stop the timer to avoid further checks
                    driveDetectionTimer.Stop();

                    // Optionally, you can check the specific drive here if necessary
                    // For now, we navigate to the browseFlashdrive UserControl
                    NavigateToBrowseFlashdrive();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., access to drives or file system)
                MessageBox.Show($"Error detecting flash drive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToBrowseFlashdrive()
        {
            try
            {
                // Assuming MainWindow has a ContentControl to switch between UserControls
                var mainWindow = (MainWindow)Application.Current.MainWindow;

                // Check if the 'browseFlashdrive' UserControl is already loaded
                var browseFlashdriveControl = new browseFlashdrive();
                mainWindow.MainContent.Content = browseFlashdriveControl; // Navigate to the new UserControl
            }
            catch (Exception ex)
            {
                // Handle any errors during navigation
                MessageBox.Show($"Error navigating to browseFlashdrive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            mainWindow.MainContent.Content = new HomeUserControl();
        }
    }
}
