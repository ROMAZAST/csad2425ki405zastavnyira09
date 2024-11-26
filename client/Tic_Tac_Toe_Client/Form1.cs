using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe_Client
{
    public partial class Form1 : Form
    {
        private readonly string noResponseMessage = "No response from Arduino. Please check the connection, and ensure the port and baud rate are correctly selected.";
        private int BAUD_RATE;
        private string arduinoPort;
        private SerialPort serialPort;
        private string game_mode = "mm";
        private bool game_time = true;
        private string move = "X";

        public Form1()
        {
            InitializeComponent();

            serialPort = new SerialPort();
            int[] baudRates = new int[]
            {
                300, 600, 1200, 2400, 4800, 9600, 14400,
                19200, 28800, 31250, 38400, 57600,115200
            };
            string[] comPorts = SerialPort.GetPortNames();
            comboBox2.Items.AddRange(comPorts);
            comboBox1.Items.AddRange(baudRates.Select(x => x.ToString()).ToArray());
            button1.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button2.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button3.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button4.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button5.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button6.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button7.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button8.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
            button9.Click += (sender, e) => { if (game_time && game_mode != "aar" && game_mode != "aaws") SendButtonNumber((Button)sender); };
        }


        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null) 
            {
                BAUD_RATE = int.Parse(comboBox1.SelectedItem.ToString()); 
            }
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null) 
            {
                arduinoPort = comboBox2.SelectedItem.ToString(); 
            }
           
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }

                var buttonNames = Controls.OfType<Button>().Select(button => string.IsNullOrEmpty(button.Text) ? "1" : button.Text).ToList();

                string message = $"save {game_mode} {move} {game_time} {string.Join("_", buttonNames)}";
                ConfigureSerialPort();

                try
                {
                    serialPort.Open();
                    serialPort.WriteLine(message); 
                    System.Threading.Thread.Sleep(1000);
                    string response = ReadResponse();

                    if (response == "saved")
                    {
                        MessageBox.Show("Game state successfully saved on Arduino.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Unexpected response from Arduino: {response}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("No response from Arduino. Check the connection and try again.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception serialEx)
                {
                    MessageBox.Show($"Failed to send the message: {serialEx.Message}", "Serial Port Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close(); 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }
                ConfigureSerialPort();
                serialPort.Open();
                serialPort.WriteLine("load");
                string response = serialPort.ReadLine().Trim();
                if (string.IsNullOrEmpty(response))
                {
                    MessageBox.Show("No data received from Arduino.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parts = response.Split(' ');
                if (parts.Length < 4)
                {
                    MessageBox.Show($"Invalid data received: {response}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                game_mode = parts[0];
                move = parts[1];
                game_time = parts[2] == "True"; 
                var buttonStates = parts[3].Split('_');

               
                label4.Text = game_mode == "ma" ? "Man vs AI" : game_mode == "mm" ? "Man vs Man" : "Unknown Mode";
                label3.Text = game_time ? (game_mode == "ma" ? "Your turn" : $"Turn: {move}") : "Game Over";

              
                var buttons = Controls.OfType<Button>().ToList();
                if (buttonStates.Length == buttons.Count)
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        if (buttonStates[i] != "1")
                            buttons[i].Text = buttonStates[i];
                    }
                    MessageBox.Show("Game state successfully loaded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Mismatch between button states and available buttons.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show("No response from Arduino. Check the connection and try again.", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game state: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close(); 
                }
            }
        }

        private void InitializeGame(string mode, string label4Text, string label3Text, string startingMove)
        {
            label4.Text = label4Text; 
            label3.Text = label3Text; 
            game_mode = mode;         
            ClearButtonValues();      
            game_time = true;         
            move = startingMove;      
        }

        private void ManVsManToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeGame("mm", "Man vs Man", "Touch to play", "X");
        }

        private void ManVsAIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeGame("ma", "Man vs AI", "Your turn", "X");
        }

        private void ClearButtonValues()
        {
            button1.Text = string.Empty;
            button2.Text = string.Empty;
            button3.Text = string.Empty;
            button4.Text = string.Empty;
            button5.Text = string.Empty;
            button6.Text = string.Empty;
            button7.Text = string.Empty;
            button8.Text = string.Empty;
            button9.Text = string.Empty;
        }

        private string ReadResponse()
        {
            string response = null;
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    response = serialPort.ReadLine().Trim(); // Читання відповіді
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading the response: " + ex.Message);
            }
            return response;
        }

        private void SendButtonNumber(Button clickedButton)
        {
            try
            {
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }
                ConfigureSerialPort();
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }

                string buttonName = clickedButton.Name;  // Ім'я кнопки
                string buttonNumber = buttonName.Replace("button", "");  // Видаляємо "button" з імені, залишаючи тільки цифру
                Button targetButton = this.Controls["button" + buttonNumber] as Button;
                string message = BuildMessageWithButtonText(game_mode, buttonNumber, move);
                serialPort.WriteLine(message);
                System.Threading.Thread.Sleep(1000);
                if (serialPort.BytesToRead > 0)
                {
                    string response = ReadResponse();
                    if (response == "winner")
                    {
                        label3.Text = "Winner: " + move;
                        targetButton.Text = move;
                        game_time = false;
                    }
                    else if (response.StartsWith("awinner"))
                    {
                       
                        string[] parts = response.Split(' ');
                        string next_move = parts[2];
                        Button targetButton1 = this.Controls["button" + parts[1]] as Button;
                        label3.Text = "Winner: O (AI)";
                        targetButton1.Text = next_move;
                        targetButton.Text = move;
                        
                        game_time = false;
                    }
                    else if (response != "declined")
                    {

                        // Перевірка, чи повідомлення починається з "confirm"
                        if (response.StartsWith("confirm"))
                        {
                            // Розбиваємо повідомлення на частини
                            string[] parts = response.Split(' ');
                            string next_move = parts[2];
                            if (targetButton != null && parts.Length <= 3)
                            {
                                targetButton.Text = move;
                                move = next_move;
                                label3.Text = "Turn " + move;
                                
                            }
                            else if (targetButton != null && parts.Length > 3)

                            {
                                Button targetButton1 = this.Controls["button" + parts[3]] as Button;
                                
                                targetButton.Text = move;
                                targetButton1.Text = next_move;
                            }


                        }
                        
                        else if (response == "draw")
                        {
                            game_time = false;
                            label3.Text = "Draw";
                            targetButton.Text = move;
                        }
                        

                    }
                    
                    
                }
                else
                {
                    MessageBox.Show(noResponseMessage);
                }
                serialPort.Close();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to open the port. Check if it is being used by another program.");
            }
            catch (IOException)
            {
                MessageBox.Show(noResponseMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private string BuildMessageWithButtonText(string game_mode, string button_number, string move)
        {
            string resultMessage = game_mode + " " + button_number + " " + move + " ";
            bool firstButton = true;  

            for (int i = 1; i <= 9; i++)
            {
                if (this.Controls["button" + i] is Button btn)
                {
                    string buttonText = string.IsNullOrEmpty(btn.Text) ? "1" : btn.Text;
                    if (!firstButton)
                    {
                        resultMessage += "_";
                    }
                    resultMessage += buttonText;
                    firstButton = false;
                }
            }

            return resultMessage;
        }
        private void ConfigureSerialPort()
        {
            serialPort.PortName = arduinoPort;
            serialPort.BaudRate = BAUD_RATE;
        }

        private async void RandomMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
            {
                MessageBox.Show("Please select a valid COM port and baud rate.");
                return;
            }

            try
            {
                ConfigureSerialPort();
                if (!serialPort.IsOpen) serialPort.Open();
                game_mode = "aar"; 
                label4.Text = "AI vs AI";
                label3.Text = "AI move(R)";
                ClearButtonValues();

                game_time = true;

                while (game_time)
                {
                    string buttonStates = string.Join("_", Enumerable.Range(1, 9).Select(i => this.Controls["button" + i] is Button btn && !string.IsNullOrEmpty(btn.Text) ? btn.Text : "1"));

                    string message = $"aar {move} {move} {buttonStates}";
                    serialPort.WriteLine(message);

                    await Task.Delay(1000);
                    string response = ReadResponse();
                    if (string.IsNullOrWhiteSpace(response))
                    {
                        MessageBox.Show(noResponseMessage);
                        break;
                    }

                    try 
                    { 
                        ProcessArduinoResponse(response);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing response: {ex.Message}\nResponse: {response}");
                        break;
                    }
                    
                }

                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private async void WinStrategyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
            {
                MessageBox.Show("Please select a valid COM port and baud rate.");
                return;
            }

            try
            {
                ConfigureSerialPort();
                if (!serialPort.IsOpen) serialPort.Open();
                game_mode = "aaws"; // AI проти AI
                label4.Text = "AI vs AI";
                label3.Text = "AI move(WS)";
                ClearButtonValues();

                game_time = true;

                while (game_time)
                {
                    string buttonStates = string.Join("_", Enumerable.Range(1, 9).Select(i => this.Controls["button" + i] is Button btn && !string.IsNullOrEmpty(btn.Text) ? btn.Text : "1"));
                    string message = $"aaws {move} {move} {buttonStates}";
                    serialPort.WriteLine(message);

                    await Task.Delay(1000);
                    string response = ReadResponse();
                    if (string.IsNullOrWhiteSpace(response))
                    {
                        MessageBox.Show(noResponseMessage);
                        break;
                    }

                    try
                    {
                        ProcessArduinoResponse(response);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing response: {ex.Message}\nResponse: {response}");
                        break;
                    }

                }

                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private void ProcessArduinoResponse(string response)
        {
            var parts = response.Split(' ');
            if (response.StartsWith("awinner") && parts.Length >= 3)
            {
               
                string winnerMove = parts[2];
                var winnerButton = Controls["button" + parts[1]];
                if (winnerButton is Button button) button.Text = winnerMove;

                label3.Text = $"Winner: AI ({winnerMove})";
                game_time = false;
            }
            else if (response == "draw")
            {
                label3.Text = "Draw";
                game_time = false;
            }
            else if (response.StartsWith("confirm") && parts.Length >= 4)
            {
                
                string nextMove = parts[3];
                label3.Text = $"Turn AI ({nextMove})";
                var aiButton = Controls["button" + parts[2]];
                if (aiButton is Button button) button.Text = move;

                move = nextMove;
            }
            else
            {
                throw new InvalidOperationException("Unexpected response format.");
            }
        }

       
    }

}
