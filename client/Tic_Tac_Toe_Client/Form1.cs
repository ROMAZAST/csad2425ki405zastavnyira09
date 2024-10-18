using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe_Client
{
    public partial class Form1 : Form
    {
        const int BAUD_RATE = 9600;
        private SerialPort serialPort;
        public Form1()
        {
            string arduinoPort = null;
            int attempt = 1;
            do
            {
                arduinoPort = FindArduinoPort();

                if (arduinoPort != null)
                {
                    MessageBox.Show($"Arduino found on port {arduinoPort}.");
                    serialPort = new SerialPort(arduinoPort, BAUD_RATE); // Set your COM port
                }
                else
                {
                    // Display a message if Arduino is not found on the current attempt
                    MessageBox.Show($"Arduino not found. Attempt {attempt} of {3}.");
                    attempt++; // Increment the attempt number
                }
            }
            while (arduinoPort == null && attempt <= 3);

            if (arduinoPort == null)
            {
                // If Arduino is not found after 3 attempts
                MessageBox.Show("Arduino not found after 3 attempts.");
                Environment.Exit(0); // Terminate the program
            }
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Sending commands to Arduino
                serialPort.Open();
                serialPort.WriteLine("LED_ON");  // For example, turn on the LED
                System.Threading.Thread.Sleep(1000); // Delay to receive a response
                ReadResponse();
                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Sending commands to Arduino
                serialPort.Open();
                serialPort.WriteLine("LED_OFF");  // For example, turn off the LED
                System.Threading.Thread.Sleep(1000); // Delay to receive a response
                ReadResponse();
                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private string FindArduinoPort()
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                try
                {
                    SerialPort testPort = new SerialPort(port, BAUD_RATE);
                    testPort.Open();

                    // Sending the request "Arduino?"
                    testPort.WriteLine("Arduino?");

                    // Waiting for a response
                    System.Threading.Thread.Sleep(1000);  // Wait for 1 second

                    if (testPort.BytesToRead > 0)
                    {
                        string response = testPort.ReadLine().Trim();  // Read the response

                        if (response == "Yes")  // If the response is "Yes", this is Arduino
                        {
                            testPort.Close();
                            return port;
                        }
                    }
                    testPort.Close();
                }
                catch
                {
                    // Ignore ports that cannot be opened
                }
            }
            return null; // If Arduino is not found
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
