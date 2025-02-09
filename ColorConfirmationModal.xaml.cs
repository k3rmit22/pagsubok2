using System.Windows;

namespace kiosk_snapprint
{
    public partial class ColorConfirmationModal : Window
    {
        public string SelectedColorStatus { get; set; } // Allows pre-setting and updating the value

        public ColorConfirmationModal()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("ColorConfirmationModal initialized.");
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // User chose to print in colored
            SelectedColorStatus = "colored";
            this.DialogResult = true; // Close the modal and confirm
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // User chose to print in greyscale
            SelectedColorStatus = "greyscale";
            this.DialogResult = true; // Close the modal and confirm
        }
    }
}
