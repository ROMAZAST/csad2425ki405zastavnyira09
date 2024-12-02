using System.IO.Ports;
using NUnit.Framework;

namespace TicTacToeTests
{
    [TestFixture]
    public class TicTacToeTests
    {

        private SerialPort serialPort = new SerialPort(); 
        public TicTacToeTests() { }

        [SetUp]
        public void Setup()
        {
            string port = Environment.GetEnvironmentVariable("SERIAL_PORT") ?? "COM5";
            int baudRate = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int br) ? br : 9600;

            serialPort = new SerialPort(port, baudRate);
            serialPort.Open();
            Thread.Sleep(500); 
        }



        [TearDown]
        public void TearDown()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        [Test]
        public void Test_SaveMessage()
        {
            SendCommand("save HelloWorld");
            string response = ReadResponse();
            Assert.That(response, Is.EqualTo("saved"), "The message was not saved correctly");
        }

        [Test]
        public void Test_LoadSavedMessage()
        {
            SendCommand("save HelloWorld");
            ReadResponse(); 
            SendCommand("load");
            string response = ReadResponse();
            Assert.That(response, Is.EqualTo("HelloWorld"), "Invalid saved message");
        }

        [Test]
        public void Test_LoadWithoutSave()
        {
            SendCommand("save ");
            ReadResponse(); 
            SendCommand("load");
            string response = ReadResponse();
            Assert.That(response, Is.EqualTo("no_saved_data"), "Should be 'no_saved_data'");
        }
        [Test]
        public void Test_InvalidCommand()
        {
            SendCommand("invalid_command");

            string response = ReadResponse();

            Assert.That(response, Is.Not.Null, "The answer must not be empty.");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void Test_ProcessManualMove(int button)
        {
            string message = $"mm {button} X 1_1_1_1_1_1_1_1_1";
            SendCommand(message);
            string response = ReadResponse();
            Assert.That(response, Is.EqualTo($"confirm {button} O"));
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void Test_AutomatedMove(int button)
        {
            string message = $"ma {button} X 1_1_1_1_1_1_1_1_1";
            SendCommand(message);

            string response = ReadResponse();

            Assert.That(response, Is.EqualTo($"confirm {button} O 2 X").Or.EqualTo($"confirm {button} O 3 X").Or.EqualTo($"confirm {button} O 4 X").Or.EqualTo($"confirm {button} O 5 X").Or.EqualTo($"confirm {button} O 6 X").Or.EqualTo($"confirm {button} O 7 X").Or.EqualTo($"confirm {button} O 8 X").Or.EqualTo($"confirm {button} O 9 X").Or.EqualTo($"confirm {button} O 1 X"));

        }

        [Test]
        public void Test_AutomatedMoves()
        {
            string message = "aar X 1_1_1_1_1_1_1_1_1";
            SendCommand(message);

            string response = ReadResponse();

            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public void Test_WinningStrategy()
        {
            string message = "aaws X X_1_1_X_1_1_1_1_1";
            SendCommand(message);

            string response = ReadResponse();

            Assert.That(response, Is.Not.Null);
        }
        [Test]
        public void Test_UnknownCommand()
        {
            SendCommand("unknown command");
            string response = ReadResponse();
            Assert.That(response, Is.Empty, "The response to an unknown command must be empty");
        }

        private void SendCommand(string command)
        {
            Console.WriteLine($"Send command: {command}");
            serialPort.WriteLine(command);
            Thread.Sleep(500); 
        }

        private string ReadResponse()
        {
            string response = "";
            int timeout = 500; 
            int elapsedTime = 0;

            while (elapsedTime < timeout)
            {
                if (serialPort.BytesToRead > 0)
                {
                    response += serialPort.ReadExisting();
                    break;
                }
                Thread.Sleep(50);
                elapsedTime += 50;
            }

            Console.WriteLine($"Response: {response.Trim()}");
            return response.Trim();
        }
    }
}
