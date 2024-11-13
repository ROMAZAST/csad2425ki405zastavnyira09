using Moq;
using System.IO.Ports;

// Interface to abstract serial port communication
public interface ISerialPort
{
    string[] GetPortNames();  // Method to get a list of available serial ports
    void Open();              // Method to open the serial port
    void Close();             // Method to close the serial port
    void WriteLine(string text); // Method to write a line of text to the serial port
    string ReadLine();        // Method to read a line of text from the serial port
    int BytesToRead { get; }  // Property to get the number of bytes available to read from the serial port
}

// Real implementation of the ISerialPort interface, interacting with the actual SerialPort class
public class RealSerialPort : ISerialPort
{
    private SerialPort _serialPort;  // Real SerialPort object

    // Constructor initializes the RealSerialPort with the specified port name
    public RealSerialPort(string portName)
    {
        _serialPort = new SerialPort(portName);
    }

    // Get a list of available serial ports
    public string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    // Open the serial port
    public void Open() => _serialPort.Open();

    // Close the serial port
    public void Close() => _serialPort.Close();

    // Write a line of text to the serial port
    public void WriteLine(string text) => _serialPort.WriteLine(text);

    // Read a line of text from the serial port
    public string ReadLine() => _serialPort.ReadLine();

    // Get the number of bytes available to read from the serial port
    public int BytesToRead => _serialPort.BytesToRead;
}

// Form1 class, which will use the ISerialPort interface to find Arduino connected to a serial port
public class Form1
{
    private ISerialPort serialPort;  // Reference to the ISerialPort interface

    // Constructor to inject the ISerialPort dependency
    public Form1(ISerialPort serialPort)
    {
        this.serialPort = serialPort;  // Assign the injected serialPort to the class field
    }

    // Method to find the Arduino by checking each available port
    public string FindArduinoPort()
    {
        string[] ports = serialPort.GetPortNames();  // Get a list of available serial ports
        foreach (string port in ports)
        {
            try
            {
                serialPort.Open();  // Try to open the serial port
                serialPort.WriteLine("Arduino?");  // Send a test message to the device
                Thread.Sleep(1000);  // Wait for a response from the device

                if (serialPort.BytesToRead > 0)  // Check if there's any data available to read
                {
                    string response = serialPort.ReadLine().Trim();  // Read the response
                    if (response == "Yes")  // If the response is "Yes", it's an Arduino
                    {
                        serialPort.Close();  // Close the serial port
                        return port;  // Return the port where Arduino was found
                    }
                }
                serialPort.Close();  // Close the serial port if no response
            }
            catch
            {
                // Ignore any exceptions (e.g., if the port cannot be opened)
            }
        }
        return null;  // Return null if no Arduino was found
    }
}


namespace TicTacToeTests
{
    public class Form1Tests
    {
        private Mock<ISerialPort> mockSerialPort;
        private Form1 form;

        public Form1Tests()
        {
            mockSerialPort = new Mock<ISerialPort>();
            form = new Form1(mockSerialPort.Object); // Initialize form with mocked serial port
        }

        // Test 1: FindArduinoPort should return a valid port when Arduino is found
        [Fact]
        public void FindArduinoPort_ShouldReturnPort_WhenArduinoIsFound()
        {
            // Arrange: Set up the mock to simulate a response from Arduino on COM3
            mockSerialPort.Setup(sp => sp.GetPortNames()).Returns(new[] { "COM3" });
            mockSerialPort.Setup(sp => sp.Open()).Verifiable();
            mockSerialPort.Setup(sp => sp.WriteLine(It.IsAny<string>())).Verifiable();
            mockSerialPort.Setup(sp => sp.ReadLine()).Returns("Yes"); // Simulate Arduino response
            mockSerialPort.Setup(sp => sp.BytesToRead).Returns(1);

            // Act: Call FindArduinoPort method
            string port = form.FindArduinoPort();

            // Assert: Verify that the correct port was returned
            Assert.Equal("COM3", port);
            mockSerialPort.Verify(sp => sp.Open(), Times.Once); // Verify Open was called
            mockSerialPort.Verify(sp => sp.WriteLine(It.IsAny<string>()), Times.Once);
            mockSerialPort.Verify(sp => sp.Close(), Times.Once); // Verify Close was called
        }

        // Test 2: FindArduinoPort should return null when no Arduino is found
        [Fact]
        public void FindArduinoPort_ShouldReturnNull_WhenNoArduinoIsFound()
        {
            // Arrange: Set up mock to simulate no response from any port
            mockSerialPort.Setup(sp => sp.GetPortNames()).Returns(new[] { "COM3", "COM4" });
            mockSerialPort.Setup(sp => sp.Open()).Verifiable();
            mockSerialPort.Setup(sp => sp.WriteLine(It.IsAny<string>())).Verifiable();
            mockSerialPort.Setup(sp => sp.ReadLine()).Returns(""); // Simulate no response from Arduino
            mockSerialPort.Setup(sp => sp.BytesToRead).Returns(0);

            // Act: Call FindArduinoPort method
            string port = form.FindArduinoPort();

            // Assert: Verify that no port was found
            Assert.Null(port);
            mockSerialPort.Verify(sp => sp.Open(), Times.Exactly(2)); // Open called for both ports
            mockSerialPort.Verify(sp => sp.WriteLine(It.IsAny<string>()), Times.Exactly(2));
            mockSerialPort.Verify(sp => sp.Close(), Times.Exactly(2)); // Close called for both ports
        }

       
        
      
    }
}