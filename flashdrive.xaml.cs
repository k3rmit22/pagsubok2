using System;
using System.Diagnostics;
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

        private bool IsValidPdf(string filePath)
        {
            try
            {
                // Read the first few bytes of the file to verify it's a PDF
                byte[] buffer = new byte[5];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string header = BitConverter.ToString(buffer);
                return header == "25-50-44-46-2D"; // PDF signature: %PDF-
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidFileName(string fileName)
        {
            return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                   fileName.Count(c => c == '.') == 1; // Ensures no hidden double extensions
        }

        private bool IsSafeFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return !fileInfo.Attributes.HasFlag(FileAttributes.Hidden) &&
                   !fileInfo.Attributes.HasFlag(FileAttributes.System);
        }



        private bool ScanWithDefender(string filePath)
        {
            string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
            if (!File.Exists(defenderPath)) return false; // Defender not found

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = defenderPath,
                    Arguments = $"-Scan -ScanType 3 -File \"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0; // 0 = No threats found
        }

        private bool ContainsMaliciousFiles(string drivePath)
        {
            var maliciousExtensions = new[] { ".exe", ".bat", ".cmd", ".vbs", ".js", ".inf" };

            foreach (var file in Directory.GetFiles(drivePath, "*.*", SearchOption.AllDirectories))
            {
                if (maliciousExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    return true; // Found a dangerous file
                }
            }
            return false;
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
