using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace Tic_Tac_Toe_Client
{
    public partial class Form1 : Form
    {
        const int BAUD_RATE = 9600;
        private SerialPort serialPort;

        public Form1()
        {
            string arduinoPort;
            int attempt = 1;
            do
            {
                arduinoPort = FindArduinoPort();

                if (arduinoPort != null)
                {
                    MessageBox.Show($"Arduino found on port {arduinoPort}.");
                    serialPort = new SerialPort(arduinoPort, BAUD_RATE); // Set the COM port for Arduino connection
                }
                else
                {
                    // Display a message if Arduino is not found in the current attempt
                    MessageBox.Show($"Arduino not found. Attempt {attempt} of {3}.");
                    attempt++; // Increment the attempt number
                }
            }
            while (arduinoPort == null && attempt <= 3); // Retry 3 times

            if (arduinoPort == null)
            {
                // If Arduino is not found after 3 attempts, exit the program
                MessageBox.Show("Arduino not found after 3 attempts.");
                Environment.Exit(0); // Terminate the program
            }
            InitializeComponent(); // Initialize the form components
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Can be used for additional form load logic if needed
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if the port is already open
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }

                // Send a command to Arduino (example: turning on LED)
                serialPort.WriteLine("LED_ON");
                System.Threading.Thread.Sleep(1000); // Delay to allow Arduino to respond

                // Check if there are any bytes to read from the port
                if (serialPort.BytesToRead > 0)
                {
                    ReadResponse(); // Read the response from Arduino
                }
                else
                {
                    MessageBox.Show("Arduino did not respond. Please check the connection.");
                }

                serialPort.Close(); // Close the serial port after communication
            }
            catch (UnauthorizedAccessException)
            {
                // If the port is blocked by another application
                MessageBox.Show("Unable to open the port. Please check if it is being used by another program.");
            }
            catch (IOException)
            {
                // If Arduino is disconnected or the port is not accessible
                MessageBox.Show("No connection to Arduino. Please check the connection.");
            }
            catch (Exception ex)
            {
                // General exception handling for any other errors
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Send commands to Arduino (example: turning off the LED)
                serialPort.Open();
                serialPort.WriteLine("LED_OFF");
                System.Threading.Thread.Sleep(1000); // Wait for a response
                ReadResponse(); // Read the response
                serialPort.Close(); // Close the port after communication
            }
            catch (UnauthorizedAccessException)
            {
                // Handle case if the port is in use by another program
                MessageBox.Show("Unable to open the port. Please check if it is being used by another program.");
            }
            catch (IOException)
            {
                // Handle cases where Arduino is disconnected
                MessageBox.Show("No connection to Arduino. Please check the connection.");
            }
            catch (Exception ex)
            {
                // Handle other general exceptions
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string message = textBox1.Text;  // Get the message from the text box

                serialPort.Open();  // Open the serial port
                serialPort.WriteLine(message);  // Send the message to Arduino
                System.Threading.Thread.Sleep(1000);  // Wait for a response
                ReadResponse();  // Read the response from Arduino
                serialPort.Close();  // Close the serial port
            }
            catch (UnauthorizedAccessException)
            {
                // Handle case if the port is in use by another program
                MessageBox.Show("Unable to open the port. Please check if it is being used by another program.");
            }
            catch (IOException)
            {
                // Handle cases where Arduino is disconnected
                MessageBox.Show("No connection to Arduino. Please check the connection.");
            }
            catch (Exception ex)
            {
                // Handle other general exceptions
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private string FindArduinoPort()
        {
            string[] ports = SerialPort.GetPortNames(); // Get all available serial ports

            foreach (string port in ports)
            {
                try
                {
                    SerialPort testPort = new SerialPort(port, BAUD_RATE);
                    testPort.Open();

                    // Sending a test request to Arduino
                    testPort.WriteLine("Arduino?");

                    // Wait for a response (1 second)
                    System.Threading.Thread.Sleep(1000);

                    if (testPort.BytesToRead > 0)
                    {
                        string response = testPort.ReadLine().Trim();  // Read the response

                        if (response == "Yes")  // If Arduino responds with "Yes"
                        {
                            testPort.Close();
                            return port; // Return the correct port
                        }
                    }
                    testPort.Close();
                }
                catch
                {
                    // Ignore ports that cannot be opened
                }
            }
            return null; // Return null if Arduino is not found
        }

        private void ReadResponse()
        {
            // Read the response from the serial port
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    string response = serialPort.ReadLine().Trim(); // Read the response
                    MessageBox.Show(response); // Show the response in a message box
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading the response: " + ex.Message);
            }
        }

    }
}
