#pragma warning disable CS8602 
using Tic_Tac_Toe_Client; 
namespace GUI_TTT_Tests
{
    [TestFixture]
    public class GUI_TTT_Tests
    {
        [TestCase("ma", "Man vs AI", "Your turn", "X")]
        [TestCase("mm", "Man vs Man", "Turn O", "O")]
        [TestCase("mm", "Man vs Man", "Touch to play", "X")] 
        public void InitializeGame_SetsCorrectValues_ModeAndLabels(
            string expectedMode,
            string expectedLabel4Text,
            string expectedLabel3Text,
            string expectedStartingMove)
        {
            var form = new Form1();
            var label4 = new Label();
            var label3 = new Label();

            form.label4 = label4;
            form.label3 = label3;

            form.InitializeGame(expectedMode, expectedLabel4Text, expectedLabel3Text, expectedStartingMove);

            Assert.Multiple(() =>
            {
                Assert.That(form.game_mode, Is.EqualTo(expectedMode), "Game mode was not set correctly.");
                Assert.That(label4.Text, Is.EqualTo(expectedLabel4Text), "label4 was not updated correctly.");
                Assert.That(label3.Text, Is.EqualTo(expectedLabel3Text), "label3 was not updated correctly.");
                Assert.That(form.move, Is.EqualTo(expectedStartingMove), "Starting move was not set correctly.");
            });

        }

        [TestCase("ma", "1", "X", "", "1_1_1_1_1_1_1_1_1")]
        [TestCase("ma", "1", "X", "X", "X_1_1_1_1_1_1_1_1")]
        [TestCase("ma", "1", "X", "X,X,X,X,X,X,X,X", "X_X_X_X_X_X_X_X_1")]
        public void BuildMessageWithButtonText_ReturnsCorrectMessage(
            string game_mode, string button_number, string move,
            string buttonText, string expectedMessage)
        {
            var form = new Form1();

            for (int i = 1; i <= 9; i++)
            {
                var button = new Button { Name = "button" + i, Text = buttonText }; 
                form.Controls.Add(button);
            }

            if (!string.IsNullOrEmpty(buttonText))
            {
                string[] buttonTexts = buttonText.Split(',');
                for (int i = 0; i < buttonTexts.Length; i++)
                {
                    form.Controls["button" + (i + 1)].Text = buttonTexts[i];
                }
            }

            string message = form.BuildMessageWithButtonText(game_mode, button_number, move);

            Assert.That(message, Is.EqualTo($"{game_mode} {button_number} {move} {expectedMessage}"));
        }

        [TestCase("awinner 1 X", "Winner: AI (X)", false)]  
        [TestCase("draw", "Draw", false)] 
        public void ProcessArduinoResponse_CorrectlyProcessesResponse(
            string response, string expectedLabel3Text, bool expectedGameTime)
        {
            var form = new Form1(); 
            
            for (int i = 1; i <= 9; i++)
            {
                var button = new Button { Name = "button" + i };
                form.Controls.Add(button);
            }

            form.ProcessArduinoResponse(response);

            Assert.Multiple(() =>
            {
                Assert.That(form.label3.Text, Is.EqualTo(expectedLabel3Text), "label3 was not updated correctly.");
                Assert.That(form.game_time, Is.EqualTo(expectedGameTime), "game_time was not updated correctly.");
            });

            if (response.StartsWith("awinner") || response.StartsWith("confirm"))
            {
                var parts = response.Split(' ');
                var button = form.Controls["button" + parts[1]] as Button;
                Assert.That(button?.Text, Is.EqualTo(parts[2]), "Button text was not updated correctly.");
            }

        }

        [TestCase("button1")]
        [TestCase("button2")]
        [TestCase("button3")]
        [TestCase("button4")]
        [TestCase("button5")]
        [TestCase("button6")]
        [TestCase("button7")]
        [TestCase("button8")]
        [TestCase("button9")]
        public void ClearButtonValues_ClearsAllButtonTexts(string buttonName)
        {
            var form = new Form1();
            var button = form.Controls[buttonName] as Button;
            button.Text = "Test";  

            form.ClearButtonValues();

            Assert.That(button.Text, Is.Empty, $"{buttonName} text was not cleared.");
        }

        [TestCase("button1", "X")]
        [TestCase("button2", "O")]
        [TestCase("button3", "X")]
        [TestCase("button4", "O")]
        [TestCase("button5", "X")]
        public void ClearButtonValues_ClearsButtonText(string buttonName, string initialText)
        {
            var form = new Form1();
            var button = form.Controls[buttonName] as Button;
            button.Text = initialText;

            form.ClearButtonValues();

            Assert.That(button.Text, Is.Empty, $"{buttonName} text was not cleared.");
        }

        [TestCase("9600", 9600)]
        [TestCase("19200", 19200)]
        [TestCase("38400", 38400)]
        [TestCase("57600", 57600)]
        [TestCase("115200", 115200)]
        public void ComboBox1_SelectedIndexChanged_UpdatesBaudRate_WhenItemIsSelected(string selectedBaudRate, int expectedBaudRate)
        {
            var form = new Form1();
            var comboBox1 = new ComboBox();
            form.comboBox1 = comboBox1;

            comboBox1.Items.Add(selectedBaudRate);
            comboBox1.SelectedItem = selectedBaudRate;

            form.ComboBox1_SelectedIndexChanged(comboBox1, EventArgs.Empty);

            Assert.That(form.BAUD_RATE, Is.EqualTo(expectedBaudRate), "BAUD_RATE was not updated correctly.");

        }

        [TestCase("COM3", "COM3")]
        [TestCase("COM4", "COM4")]
        [TestCase("COM5", "COM5")]
        [TestCase("COM6", "COM6")]
        [TestCase("COM7", "COM7")]
        public void ComboBox2_SelectedIndexChanged_UpdatesArduinoPort_WhenItemIsSelected(string selectedPort, string expectedPort)
        {
            var form = new Form1();
            var comboBox2 = new ComboBox();
            form.comboBox2 = comboBox2;

            comboBox2.Items.Add(selectedPort);
            comboBox2.SelectedItem = selectedPort;

            form.ComboBox2_SelectedIndexChanged(comboBox2, EventArgs.Empty);

            Assert.That(form.arduinoPort, Is.EqualTo(expectedPort), "arduinoPort was not updated correctly.");

        }



    }
}
