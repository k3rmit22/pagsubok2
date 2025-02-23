using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for cancel_transaction_modal.xaml
    /// </summary>
    public partial class cancel_transaction_modal : Window
    {
        public SerialPort SecondSerialPort { get; set; } // To send commands to COM9

        public cancel_transaction_modal()
        {
            InitializeComponent();
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondSerialPort != null && SecondSerialPort.IsOpen)
            {
                try
                {
                    // Example: Send a command to the hardware (e.g., reset servo or cancel transaction)
                    string cancelCommand = "servo180";  // Replace this with your specific command

                    // Call an asynchronous method to write to the serial port
                    await Task.Run(() => SecondSerialPort.WriteLine(cancelCommand));

                    Debug.WriteLine($"Sent command to hardware via COM5: {cancelCommand}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending command to hardware: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"Error sending command: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("The second serial port is not open or available.", "Port Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Navigate to HomeUserControl after the action
            NavigateToHomeUserControl();

            // Logic for "Yes" button, for example, closing the modal after sending the command
            this.Close();
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
