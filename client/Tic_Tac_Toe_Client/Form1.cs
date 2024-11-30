using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe_Client
{
    /// <summary>
    /// Main form for the Tic-Tac-Toe game that connects to Arduino via serial port.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Message displayed when there's no response from Arduino.
        /// </summary>
        public readonly string noResponseMessage = "No response from Arduino. Please check the connection, and ensure the port and baud rate are correctly selected.";

        /// <summary>
        /// Baud rate for the serial port.
        /// </summary>
        public int BAUD_RATE;

        /// <summary>
        /// The port to which Arduino is connected.
        /// </summary>
        public string arduinoPort;

        /// <summary>
        /// Serial port object.
        /// </summary>
        public SerialPort serialPort;

        /// <summary>
        /// Game mode identifier.
        /// </summary>
        public string game_mode = "mm";

        /// <summary>
        /// Indicates whether the game is still ongoing.
        /// </summary>
        public bool game_time = true;

        /// <summary>
        /// Current player's move ("X" or "O").
        /// </summary>
        public string move = "X";

        /// <summary>
        /// Constructor for initializing the form.
        /// </summary>
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
            // Initialize buttons
        }


        /// <summary>
        /// Event handler for selecting a baud rate from the combo box.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                BAUD_RATE = int.Parse(comboBox1.SelectedItem.ToString());
            }
        }

        /// <summary>
        /// Event handler for selecting an Arduino port from the combo box.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                arduinoPort = comboBox2.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Event handler for saving the game state.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a valid COM port and baud rate have been selected
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }

                // Gather the current states of all the buttons
                var buttonNames = Controls.OfType<Button>().Select(button => string.IsNullOrEmpty(button.Text) ? "1" : button.Text).ToList();

                // Build the message string to send to Arduino
                string message = $"save {game_mode} {move} {game_time} {string.Join("_", buttonNames)}";
                ConfigureSerialPort();

                try
                {
                    // Open the serial port and send the message to Arduino
                    serialPort.Open();
                    serialPort.WriteLine(message);
                    System.Threading.Thread.Sleep(1000);  // Wait for Arduino to process the message
                    string response = ReadResponse(); // Read the response from Arduino

                    // Check if the response is as expected
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
                    // Close the serial port if it's open
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any general exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the "Load" action from the menu to load the game state from Arduino.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if the COM port and baud rate are properly set
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }

                // Configure and open the serial port
                ConfigureSerialPort();
                serialPort.Open();
                serialPort.WriteLine("load");  // Send 'load' command to Arduino

                // Read the response from Arduino
                string response = serialPort.ReadLine().Trim();
                if (string.IsNullOrEmpty(response))
                {
                    MessageBox.Show("No data received from Arduino.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Split the response into parts
                var parts = response.Split(' ');
                if (parts.Length < 4)
                {
                    MessageBox.Show($"Invalid data received: {response}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Set the game mode, move, and game time based on the received data
                game_mode = parts[0];
                move = parts[1];
                game_time = parts[2] == "True";  // Parse game time (True/False)
                var buttonStates = parts[3].Split('_');  // Parse button states

                // Update labels based on the game mode
                label4.Text = game_mode == "ma" ? "Man vs AI" : game_mode == "mm" ? "Man vs Man" : "Unknown Mode";
                label3.Text = game_time ? (game_mode == "ma" ? "Your turn" : $"Turn: {move}") : "Game Over";

                // Update the buttons based on the button states received
                var buttons = Controls.OfType<Button>().ToList();
                if (buttonStates.Length == buttons.Count)
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        if (buttonStates[i] != "1")  // If the state is not "1", update the button text
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
                // Close the serial port if it is open
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
        }

        /// <summary>
        /// Initializes the game with the specified mode, label texts, and starting move.
        /// </summary>
        /// <param name="mode">The game mode (e.g., "mm" for Man vs Man, "ma" for Man vs AI).</param>
        /// <param name="label4Text">Text to set for label4 (e.g., "Man vs Man", "Man vs AI").</param>
        /// <param name="label3Text">Text to set for label3 (e.g., "Touch to play", "Your turn").</param>
        /// <param name="startingMove">The starting move for the game (e.g., "X" or "O").</param>
        public void InitializeGame(string mode, string label4Text, string label3Text, string startingMove)
        {
            label4.Text = label4Text;  // Set the text for label4
            label3.Text = label3Text;  // Set the text for label3
            game_mode = mode;          // Set the game mode
            ClearButtonValues();       // Clear button values to reset the board
            game_time = true;          // Indicate that the game is ongoing
            move = startingMove;       // Set the starting move ("X" or "O")
        }

        /// <summary>
        /// Starts a "Man vs Man" game mode when the corresponding menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void ManVsManToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeGame("mm", "Man vs Man", "Touch to play", "X"); // Initialize game for Man vs Man mode
        }

        /// <summary>
        /// Starts a "Man vs AI" game mode when the corresponding menu item is clicked.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public void ManVsAIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeGame("ma", "Man vs AI", "Your turn", "X"); // Initialize game for Man vs AI mode
        }

        /// <summary>
        /// Clears the text of all buttons to reset the game board.
        /// </summary>
        public void ClearButtonValues()
        {
            button1.Text = string.Empty;  // Clear text on button 1
            button2.Text = string.Empty;  // Clear text on button 2
            button3.Text = string.Empty;  // Clear text on button 3
            button4.Text = string.Empty;  // Clear text on button 4
            button5.Text = string.Empty;  // Clear text on button 5
            button6.Text = string.Empty;  // Clear text on button 6
            button7.Text = string.Empty;  // Clear text on button 7
            button8.Text = string.Empty;  // Clear text on button 8
            button9.Text = string.Empty;  // Clear text on button 9
        }

        /// <summary>
        /// Reads the response from Arduino after sending a message.
        /// </summary>
        /// <returns>The response from Arduino.</returns>
        public string ReadResponse()
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

        /// <summary>
        /// Event handler for sending the button number to Arduino.
        /// </summary>
        /// <param name="clickedButton">The button clicked by the user.</param>
        public void SendButtonNumber(Button clickedButton)
        {
            try
            {
                // Check if a valid COM port and baud rate are selected
                if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
                {
                    MessageBox.Show("Please select a valid COM port and baud rate.");
                    return;
                }

                // Configure and open the serial port
                ConfigureSerialPort();
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }

                // Extract the button's name and number
                string buttonName = clickedButton.Name;  // Button name (e.g., "button1")
                string buttonNumber = buttonName.Replace("button", "");  // Remove "button" from the name to get the number

                // Get the button control that corresponds to the button number
                Button targetButton = this.Controls["button" + buttonNumber] as Button;

                // Build the message to send to Arduino, including the game mode, button number, and current move
                string message = BuildMessageWithButtonText(game_mode, buttonNumber, move);
                serialPort.WriteLine(message);  // Send the message to Arduino
                System.Threading.Thread.Sleep(1000);  // Wait for Arduino to process the message

                // Check if there is data available from Arduino
                if (serialPort.BytesToRead > 0)
                {
                    // Read and handle the response from Arduino
                    string response = ReadResponse();

                    // If the response indicates a winner, update the UI and end the game
                    if (response == "winner")
                    {
                        label3.Text = "Winner: " + move;
                        targetButton.Text = move;  // Mark the button with the current move
                        game_time = false;  // End the game
                    }
                    // If the response indicates an AI winner, update the UI with the AI's move
                    else if (response.StartsWith("awinner"))
                    {
                        string[] parts = response.Split(' ');
                        string next_move = parts[2];  // The AI's next move
                        Button targetButton1 = this.Controls["button" + parts[1]] as Button;
                        label3.Text = "Winner: O (AI)";
                        targetButton1.Text = next_move;  // Mark the AI's winning move
                        targetButton.Text = move;  // Mark the current player's move
                        game_time = false;  // End the game
                    }
                    // If the response is not "declined", check for other cases
                    else if (response != "declined")
                    {
                        // If the response starts with "confirm", update the move and button states
                        if (response.StartsWith("confirm"))
                        {
                            string[] parts = response.Split(' ');
                            string next_move = parts[2];

                            // If the response contains the next move, update the UI
                            if (targetButton != null && parts.Length <= 3)
                            {
                                targetButton.Text = move;  // Mark the button with the current move
                                move = next_move;  // Update the move for the next turn
                                label3.Text = "Turn " + move;  // Update the turn label
                            }
                            // If the response contains an additional button, update that button as well
                            else if (targetButton != null && parts.Length > 3)
                            {
                                Button targetButton1 = this.Controls["button" + parts[3]] as Button;
                                targetButton.Text = move;  // Mark the current move
                                targetButton1.Text = next_move;  // Mark the next move for the opponent
                            }
                        }
                        // If the game ends in a draw
                        else if (response == "draw")
                        {
                            game_time = false;  // End the game
                            label3.Text = "Draw";  // Update the label to indicate a draw
                            targetButton.Text = move;  // Mark the button with the current move
                        }
                    }
                }
                else
                {
                    MessageBox.Show(noResponseMessage);  // Show a message if no response is received
                }

                // Close the serial port connection
                serialPort.Close();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to open the port. Check if it is being used by another program.");
            }
            catch (IOException)
            {
                MessageBox.Show(noResponseMessage);  // Handle IO exceptions (e.g., no response from Arduino)
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);  // Catch and display other errors
            }
        }

        /// <summary>
        /// Builds a message string that represents the current game state, including the game mode, 
        /// the selected button number, the current move, and the state of all game buttons.
        /// </summary>
        /// <param name="game_mode">The current game mode (e.g., "aar" for AI vs AI).</param>
        /// <param name="button_number">The number of the button that was pressed (1-9).</param>
        /// <param name="move">The current move (either "X" or "O").</param>
        /// <returns>A string formatted to represent the game state, which will be sent to Arduino.</returns>
        public string BuildMessageWithButtonText(string game_mode, string button_number, string move)
        {
            string resultMessage = game_mode + " " + button_number + " " + move + " ";
            bool firstButton = true;

            // Iterate through buttons 1 to 9 and add their text values to the message
            for (int i = 1; i <= 9; i++)
            {
                if (this.Controls["button" + i] is Button btn)
                {
                    // Use "1" if the button is empty
                    string buttonText = string.IsNullOrEmpty(btn.Text) ? "1" : btn.Text;

                    // Add an underscore between button states
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


        /// <summary>
        /// Configures the serial port with the selected parameters.
        /// </summary>
        public void ConfigureSerialPort()
        {
            serialPort.PortName = arduinoPort;
            serialPort.BaudRate = BAUD_RATE;
        }

        /// <summary>
        /// Handles the click event for the "Random Move" menu item to start an AI vs AI game 
        /// where the AI makes random moves and the game is played until completion.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public async void RandomMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
            {
                MessageBox.Show("Please select a valid COM port and baud rate.");
                return;
            }

            try
            {
                // Configure and open the serial port
                ConfigureSerialPort();
                if (!serialPort.IsOpen) serialPort.Open();
                game_mode = "aar";  // AI vs AI mode
                label4.Text = "AI vs AI";
                label3.Text = "AI move(R)";
                ClearButtonValues();  // Clear the game board

                game_time = true;

                while (game_time)
                {
                    // Build a string representing the current button states
                    string buttonStates = string.Join("_", Enumerable.Range(1, 9).Select(i => this.Controls["button" + i] is Button btn && !string.IsNullOrEmpty(btn.Text) ? btn.Text : "1"));

                    // Send the game state to Arduino
                    string message = $"aar {move} {move} {buttonStates}";
                    serialPort.WriteLine(message);

                    // Wait for 1 second
                    await Task.Delay(1000);

                    // Read response from Arduino
                    string response = ReadResponse();
                    if (string.IsNullOrWhiteSpace(response))
                    {
                        MessageBox.Show(noResponseMessage);
                        break;
                    }

                    try
                    {
                        // Process the Arduino response
                        ProcessArduinoResponse(response);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing response: {ex.Message}\nResponse: {response}");
                        break;
                    }
                }

                // Close the serial port after the game is over
                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the "Win Strategy" menu item to start an AI vs AI game 
        /// using a predefined winning strategy.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public async void WinStrategyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(arduinoPort) || BAUD_RATE <= 0)
            {
                MessageBox.Show("Please select a valid COM port and baud rate.");
                return;
            }

            try
            {
                // Configure and open the serial port
                ConfigureSerialPort();
                if (!serialPort.IsOpen) serialPort.Open();
                game_mode = "aaws"; // AI vs AI with winning strategy
                label4.Text = "AI vs AI";
                label3.Text = "AI move(WS)";
                ClearButtonValues();  // Clear the game board

                game_time = true;

                while (game_time)
                {
                    // Build a string representing the current button states
                    string buttonStates = string.Join("_", Enumerable.Range(1, 9).Select(i => this.Controls["button" + i] is Button btn && !string.IsNullOrEmpty(btn.Text) ? btn.Text : "1"));

                    // Send the game state to Arduino
                    string message = $"aaws {move} {move} {buttonStates}";
                    serialPort.WriteLine(message);

                    // Wait for 1 second
                    await Task.Delay(1000);

                    // Read response from Arduino
                    string response = ReadResponse();
                    if (string.IsNullOrWhiteSpace(response))
                    {
                        MessageBox.Show(noResponseMessage);
                        break;
                    }

                    try
                    {
                        // Process the Arduino response
                        ProcessArduinoResponse(response);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing response: {ex.Message}\nResponse: {response}");
                        break;
                    }
                }

                // Close the serial port after the game is over
                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes the response received from Arduino to update the game state, 
        /// including the game outcome, AI moves, and button states.
        /// </summary>
        /// <param name="response">The response string from Arduino, containing game state information.</param>
        /// <exception cref="InvalidOperationException">Thrown if the response format is unexpected.</exception>
        public void ProcessArduinoResponse(string response)
        {
            var parts = response.Split(' ');

            if (response.StartsWith("awinner") && parts.Length >= 3)
            {
                // Handle the case where there is a winner
                string winnerMove = parts[2];
                var winnerButton = Controls["button" + parts[1]];
                if (winnerButton is Button button) button.Text = winnerMove;

                label3.Text = $"Winner: AI ({winnerMove})";
                game_time = false;  // End the game
            }
            else if (response == "draw")
            {
                // Handle the draw case
                label3.Text = "Draw";
                game_time = false;  // End the game
            }
            else if (response.StartsWith("confirm") && parts.Length >= 4)
            {
                // Confirm the next AI move
                string nextMove = parts[3];
                label3.Text = $"Turn AI ({nextMove})";
                var aiButton = Controls["button" + parts[2]];
                if (aiButton is Button button) button.Text = move;

                move = nextMove;  // Update the move for the next turn
            }
            else
            {
                // Throw an error if the response format is unexpected
                throw new InvalidOperationException("Unexpected response format.");
            }
        }

    }

}
