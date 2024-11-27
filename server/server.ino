/** 
 * @var savedMessage
 * @brief Variable to store the saved message.
 */
String savedMessage = "";  

/** 
 * @brief Initializes the serial communication.
 */
void setup() {
  // Initialize the serial port at 9600 baud rate
  Serial.begin(9600);
}

/** 
 * @brief Main loop that reads commands from the serial port and processes them.
 */
void loop() {
  // Check if there is any data available to read from the serial port
  if (Serial.available() > 0) {
    String message = Serial.readStringUntil('\n');  // Read the message until a newline character
    message.trim();  // Remove any leading or trailing whitespace
    
    // If the message starts with "save", store it as savedMessage
    if (message.startsWith("save ")) {
      savedMessage = message.substring(5); 
      Serial.println("saved");  // Send confirmation that the message has been saved
    } 
    // If the message is "load", send the saved message or "no_saved_data" if none exists
    else if (message.startsWith("load")) {
      if (savedMessage != "") {
        Serial.println(savedMessage);  // Send the saved message
      } else {
        Serial.println("no_saved_data");  // No saved data to load
      }
    }
    else {
      // Process game-related commands if the message doesn't start with "save" or "load"
      processGameCommands(message);
    }
  }
}

/** 
 * @brief Processes game-related commands, such as making moves, checking winners, and more.
 * 
 * @param message The command received from the serial port.
 */
void processGameCommands(String message) {
  int firstSpace = message.indexOf(' ');  /**< Index of the first space in the message */
  int secondSpace = message.indexOf(' ', firstSpace + 1); /**< Index of the second space */
  int thirdSpace = message.indexOf(' ', secondSpace + 1); /**< Index of the third space */

  String gameMode = message.substring(0, firstSpace);  /**< Game mode (e.g., mm, ma, etc.) */
  String buttonNumber = message.substring(firstSpace + 1, secondSpace);  /**< Button number pressed */
  String turn = message.substring(secondSpace + 1, thirdSpace);  /**< Current player's turn (X or O) */
  String boardState = message.substring(thirdSpace + 1);  /**< Current state of the game board */

  int startIndex = 0;
  int endIndex = 0;
  int i = 0;
  String boardStateParts[9];  /**< Array to store the board state parts (buttons) */

  // Split the board state into individual button states
  while ((endIndex = boardState.indexOf('_', startIndex)) != -1 && i < 9) {
    boardStateParts[i++] = boardState.substring(startIndex, endIndex);
    startIndex = endIndex + 1;
  }
  boardStateParts[i] = boardState.substring(startIndex);  // Add the last part

  // Process different types of moves (manual, automated, etc.)
  if (message.startsWith("mm")) {
    processManualMove(boardStateParts, buttonNumber, turn);
  } else if (message.startsWith("ma")) {
    processAutomatedMove(boardStateParts, buttonNumber, turn);
  } else if (message.startsWith("aar")) {
    processAutomatedMoves(boardStateParts, turn);
  } else if (message.startsWith("aaws")) {
    if(turn == "X") {
      processWinningStrategy(boardStateParts, turn);
    } else {
      processAutomatedMoves(boardStateParts, turn);
    }
  }
}

/** 
 * @brief Processes the winning strategy for the AI.
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @param turn The current player's turn (X or O).
 */
void processWinningStrategy(String boardStateParts[9], String turn) {
  int bestMove = -1;

  // Check if the AI can win on the next move
  for (int i = 0; i < 9; i++) {
    if (boardStateParts[i] == "1") {
      boardStateParts[i] = turn;  // Simulate a move
      if (checkWinner(boardStateParts, turn)) {
        bestMove = i;  // This move leads to a win
        boardStateParts[i] = "1";  // Undo the simulation
        break;
      }
      boardStateParts[i] = "1";  // Undo the simulation
    }
  }

  // Block the opponent from winning if no winning move exists
  if (bestMove == -1) {
    String opponent = (turn == "X") ? "O" : "X";
    for (int i = 0; i < 9; i++) {
      if (boardStateParts[i] == "1") {
        boardStateParts[i] = opponent;  // Simulate the opponent's move
        if (checkWinner(boardStateParts, opponent)) {
          bestMove = i;  // Block the opponent's winning move
          boardStateParts[i] = "1";  // Undo the simulation
          break;
        }
        boardStateParts[i] = "1";  // Undo the simulation
      }
    }
  }

  // If there are no obvious winning or blocking moves, choose the first available move
  if (bestMove == -1) {
    for (int i = 0; i < 9; i++) {
      if (boardStateParts[i] == "1") {
        bestMove = i;
        break;
      }
    }
  }

  // Make the selected move
  if (bestMove != -1) {
    boardStateParts[bestMove] = turn;

    if (checkWinner(boardStateParts, turn)) {
      Serial.println("awinner " + String(bestMove + 1) + " " + turn);  // Send winner message
    } else if (isBoardFull(boardStateParts)) {
      Serial.println("draw");  // Send draw message if the board is full
    } else {
      String nextTurn = (turn == "X") ? "O" : "X";  // Alternate the turn
      Serial.println("confirm " + turn + " " + String(bestMove + 1) + " " + nextTurn);  // Confirm the move
    }
  }
}

/** 
 * @brief Processes a manual move by the player.
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @param buttonNumber The button number that was pressed.
 * @param turn The current player's turn (X or O).
 */
void processManualMove(String boardStateParts[9], String buttonNumber, String turn) {
  int buttonIndex = buttonNumber.toInt() - 1;
  if (buttonIndex >= 0 && buttonIndex < 9 && boardStateParts[buttonIndex] == "1") {
    boardStateParts[buttonIndex] = turn;

    if (checkWinner(boardStateParts, turn)) {
      Serial.println("winner");
    } else {
      bool hasEmptyCells = false;
      for (int j = 0; j < 9; j++) {
        if (boardStateParts[j] == "1") {
          hasEmptyCells = true;
          break;
        }
      }
      if (!hasEmptyCells) {
        Serial.println("draw");
      } else {
        turn = (turn == "X") ? "O" : "X";  // Switch turn
        Serial.println("confirm " + buttonNumber + " " + turn);  // Confirm the move
      }
    }
  } else {
    Serial.println("declined");  // Decline the move if it's invalid
  }
}

/** 
 * @brief Processes an automated move by the AI.
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @param buttonNumber The button number to simulate a move.
 * @param turn The current player's turn (X or O).
 */
void processAutomatedMove(String boardStateParts[9], String buttonNumber, String turn) {
  int buttonIndex = buttonNumber.toInt() - 1;
  if (buttonIndex >= 0 && buttonIndex < 9 && boardStateParts[buttonIndex] == "1") {
    boardStateParts[buttonIndex] = turn;

    if (checkWinner(boardStateParts, turn)) {
      Serial.println("winner");
    } else {
      bool hasEmptyCells = false;
      for (int j = 0; j < 9; j++) {
        if (boardStateParts[j] == "1") {
          hasEmptyCells = true;
          break;
        }
      }

      if (!hasEmptyCells) {
        Serial.println("draw");
      } else {
        // Choose a random valid button for the opponent's move
        int randomButton;
        do {
          randomButton = random(1, 10);
        } while (boardStateParts[randomButton-1] != "1");
        String turn1 = (turn == "X") ? "O" : "X";  // Alternate turn
        boardStateParts[randomButton - 1] = turn1;
        if (checkWinner(boardStateParts, turn1)) {
          Serial.println("awinner " + String(randomButton) + " " + turn1);  // Send winner message for AI
        } else {
          Serial.println("confirm " + buttonNumber + " " + turn1 + " " + String(randomButton) + " " + turn);  // Confirm the move
        }
      }
    }
  } else {
    Serial.println("declined");  // Decline the move if it's invalid
  }
}

/** 
 * @brief Processes multiple automated moves for the AI.
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @param turn The current player's turn (X or O).
 */
void processAutomatedMoves(String boardStateParts[9], String turn) {
  int randomButton;
  do {
    randomButton = random(0, 9);  // Generate a random index (0-8)
  } while (boardStateParts[randomButton] != "1");

  boardStateParts[randomButton] = turn;
  String nextTurn = (turn == "X") ? "O" : "X";  // Alternate the turn
  if (checkWinner(boardStateParts, turn)) {
    Serial.println("awinner " + String(randomButton+1) + " " + turn);  // Send winner message
  } else if (isBoardFull(boardStateParts)) {
    Serial.println("draw");  // Send draw message if the board is full
  } else {
    Serial.println("confirm " + turn + " " + String(randomButton+1) + " " + nextTurn);  // Confirm the move
  }
}

/** 
 * @brief Checks if a player has won the game.
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @param turn The current player's turn (X or O).
 * @return true if the player has won, false otherwise.
 */
bool checkWinner(String boardStateParts[9], String turn) {
  int winningCombinations[8][3] = {
    {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
    {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
    {0, 4, 8}, {2, 4, 6}
  };

  // Check all winning combinations
  for (int i = 0; i < 8; i++) {
    int a = winningCombinations[i][0];
    int b = winningCombinations[i][1];
    int c = winningCombinations[i][2];
    if (boardStateParts[a] == turn && boardStateParts[b] == turn && boardStateParts[c] == turn) {
      return true;  // Player has won
    }
  }
  return false;  // No winner yet
}

/** 
 * @brief Checks if the board is full (i.e., no empty cells).
 * 
 * @param boardStateParts Array containing the current state of the game board.
 * @return true if the board is full, false otherwise.
 */
bool isBoardFull(String boardStateParts[9]) {
  for (int i = 0; i < 9; i++) {
    if (boardStateParts[i] == "1") {
      return false;  // There is still an empty cell
    }
  }
  return true;  // Board is full
}
