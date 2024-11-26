String savedMessage = ""; 

void setup() {
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    String message = Serial.readStringUntil('\n');
    message.trim();
    
    if (message.startsWith("save ")) {
      savedMessage = message.substring(5); 
      Serial.println("saved"); 
    } 
    else if (message.startsWith("load")) {
      if (savedMessage != "") {
        Serial.println(savedMessage); 
      } else {
        Serial.println("no_saved_data"); 
      }
    }
    else {
      processGameCommands(message);
    }
  }
}

void processGameCommands(String message) {
  int firstSpace = message.indexOf(' ');
  int secondSpace = message.indexOf(' ', firstSpace + 1);
  int thirdSpace = message.indexOf(' ', secondSpace + 1);
  String gameMode = message.substring(0, firstSpace);
  String buttonNumber = message.substring(firstSpace + 1, secondSpace);
  String turn = message.substring(secondSpace + 1, thirdSpace);
  String boardState = message.substring(thirdSpace + 1);
  int startIndex = 0;
  int endIndex = 0;
  int i = 0;
  String boardStateParts[9];

  while ((endIndex = boardState.indexOf('_', startIndex)) != -1 && i < 9) {
    boardStateParts[i++] = boardState.substring(startIndex, endIndex);
    startIndex = endIndex + 1;
  }
  boardStateParts[i] = boardState.substring(startIndex);

  if (message.startsWith("mm")) {
    processManualMove(boardStateParts, buttonNumber, turn);
  } else if (message.startsWith("ma")) {
    processAutomatedMove(boardStateParts, buttonNumber, turn);
  } else if (message.startsWith("aar")) {
    processAutomatedMoves(boardStateParts, turn);
  } else if (message.startsWith("aaws")) {
    if(turn == "X"){
      processWinningStrategy(boardStateParts, turn);
    }
    else
    {
      processAutomatedMoves(boardStateParts, turn);
    }
  }
}
void processWinningStrategy(String boardStateParts[9], String turn) {
  int bestMove = -1;

  for (int i = 0; i < 9; i++) {
    if (boardStateParts[i] == "1") {
      boardStateParts[i] = turn; 
      if (checkWinner(boardStateParts, turn)) {
        bestMove = i; 
        boardStateParts[i] = "1"; 
        break;
      }
      boardStateParts[i] = "1"; 
    }
  }

  if (bestMove == -1) {
    String opponent = (turn == "X") ? "O" : "X";
    for (int i = 0; i < 9; i++) {
      if (boardStateParts[i] == "1") {
        boardStateParts[i] = opponent;
        if (checkWinner(boardStateParts, opponent)) {
          bestMove = i;
          boardStateParts[i] = "1"; 
          break;
        }
        boardStateParts[i] = "1"; 
      }
    }
  }

  if (bestMove == -1) {
    for (int i = 0; i < 9; i++) {
      if (boardStateParts[i] == "1") {
        bestMove = i;
        break;
      }
    }
  }

  if (bestMove != -1) {
    boardStateParts[bestMove] = turn;

    if (checkWinner(boardStateParts, turn)) {
      Serial.println("awinner " + String(bestMove + 1) + " " + turn);
    } else if (isBoardFull(boardStateParts)) {
      Serial.println("draw");
    } else {
      String nextTurn = (turn == "X") ? "O" : "X";
      Serial.println("confirm " + turn + " " + String(bestMove + 1) + " " + nextTurn);
    }
  }
}
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
        turn = (turn == "X") ? "O" : "X";
        Serial.println("confirm " + buttonNumber + " " + turn);
      }
    }
  } else {
    Serial.println("declined");
  }
}

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
        int randomButton;
        do {
          randomButton = random(1, 10);
        } while (boardStateParts[randomButton-1] != "1");
        String turn1 = (turn == "X") ? "O" : "X";
        boardStateParts[randomButton - 1] = turn1;
        if (checkWinner(boardStateParts, turn1)) {
          Serial.println("awinner " + String(randomButton) + " " + turn1);
        } else {
          Serial.println("confirm " + buttonNumber + " " + turn1 + " " + String(randomButton) + " " + turn);
        }
      }
    }
  } else {
    Serial.println("declined");
  }
}

void processAutomatedMoves(String boardStateParts[9], String turn) {
  int randomButton;
  do {
    randomButton = random(0, 9); 
  } while (boardStateParts[randomButton] != "1");

  boardStateParts[randomButton] = turn;
  String nextTurn = (turn == "X") ? "O" : "X";
  if (checkWinner(boardStateParts, turn)) {
    Serial.println("awinner " + String(randomButton+1) + " " + turn);
  } else if (isBoardFull(boardStateParts)) {
    Serial.println("draw");
  } else {
    
    Serial.println("confirm " + turn + " " + String(randomButton+1) + " " + nextTurn);
  }
}
bool checkWinner(String boardStateParts[9], String turn) {
  int winningCombinations[8][3] = {
    {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
    {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
    {0, 4, 8}, {2, 4, 6}
  };

  for (int i = 0; i < 8; i++) {
    int a = winningCombinations[i][0];
    int b = winningCombinations[i][1];
    int c = winningCombinations[i][2];
    if (boardStateParts[a] == turn && boardStateParts[b] == turn && boardStateParts[c] == turn) {
      return true;
    }
  }
  return false;
}
bool isBoardFull(String boardStateParts[9]) {
  for (int i = 0; i < 9; i++) {
    if (boardStateParts[i] == "1") {
      return false;
    }
  }
  return true;
}
