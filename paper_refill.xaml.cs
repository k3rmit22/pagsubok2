using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for paper_refill.xaml
    /// </summary>
    public partial class paper_refill : UserControl
    {
        // Dictionary to track the last sent command for each lifter
        private static Dictionary<int, string> lastSentCommands = new Dictionary<int, string>();

        public paper_refill()
        {
            InitializeComponent();

            // Initialize the last sent commands for lifters
            for (int lifterId = 1; lifterId <= 3; lifterId++)
            {
                lastSentCommands[lifterId] = null; // Set initial state to null
            }

            // Start fetching commands asynchronously
            StartFetchingCommands();
        }

        private async void StartFetchingCommands()
        {
            Console.WriteLine("Fetching commands for lifters...");

            while (true)
            {
                await FetchAllCommands();
                await Task.Delay(120000); // Wait for 5 seconds before fetching again
            }
        }

        private async Task FetchAllCommands()
        {
            for (int lifterId = 1; lifterId <= 3; lifterId++) // Loop through all lifters
            {
                await FetchAndProcessCommand(lifterId);
            }
        }

        private async Task FetchAndProcessCommand(int lifterId)
        {
            using (HttpClient client = new HttpClient())
            {
                // API URL with lifter ID as a query parameter
                string apiUrl = $"https://snapprintadmin.online/command.php?id={lifterId}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Lifter {lifterId} command: {jsonResponse}");

                        // Extract the command (e.g., "1,refill")
                        string rawCommand = jsonResponse.Replace("{\"command\":\"", "").Replace("\"}", "");

                        // Check if the command has changed
                        if (lastSentCommands[lifterId] != rawCommand)
                        {
                            // Update the last sent command and send it to Arduino
                            lastSentCommands[lifterId] = rawCommand;
                            SendCommandToArduino(rawCommand);

                            // Optionally update a label or UI element
                            UpdateLifterStatus(lifterId, rawCommand);
                        }
                        else
                        {
                            Debug.WriteLine($"Lifter {lifterId}: Command has not changed, skipping.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Error retrieving command for Lifter {lifterId}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching command for Lifter {lifterId}: {ex.Message}");
                }
            }
        }

        private void SendCommandToArduino(string command)
        {
            // Ensure proper COM port setup
            using (System.IO.Ports.SerialPort port = new System.IO.Ports.SerialPort("COM6", 9600))
            {
                try
                {
                    port.Open();
                    port.WriteLine(command); // Send the command to Arduino
                    Console.WriteLine($"Command sent to Arduino: {command}");
                    Debug.WriteLine($"Command Sent: {command}");
                    port.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending command to Arduino: {ex.Message}");
                }
            }
        }

        private void UpdateLifterStatus(int lifterId, string command)
        {
            // Update a Label or TextBlock in your WPF UI for each lifter
            Application.Current.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"Updating UI for Lifter {lifterId} with command: {command}");
                // Example: Use IDs to identify specific UI elements
                if (lifterId == 1)
                {
                    Lifter1Status.Text = $"Lifter 1: {command}";
                }
                else if (lifterId == 2)
                {
                    Lifter2Status.Text = $"Lifter 2: {command}";
                }
                else if (lifterId == 3)
                {
                    Lifter3Status.Text = $"Lifter 3: {command}";
                }
            });
        }
    }
}
