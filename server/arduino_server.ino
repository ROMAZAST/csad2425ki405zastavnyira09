const int BAUD_RATE = 9600;  // Define a constant for the baud rate

void setup() {
  Serial.begin(BAUD_RATE);  // Initialize the serial port
  pinMode(13, OUTPUT);  // Set pin 13 as an output for the LED
}

void loop() {
  if (Serial.available() > 0) {  // Check if data is available to read from the serial port
    String receivedMessage = Serial.readStringUntil('\n');  // Read the incoming message until newline character

    if (receivedMessage == "Arduino?") {  // Check if the message is asking for Arduino identification
      Serial.println("Yes");  // Respond with "Yes" if the query is about Arduino
    }

    if (receivedMessage == "LED_ON") {  // If the received message is "LED_ON"
      digitalWrite(13, HIGH);  // Turn on the LED connected to pin 13
      Serial.println("Command " + receivedMessage + " is completed successfully");  // Send a success response after executing the command
    } 
    else if (receivedMessage == "LED_OFF") {  // If the received message is "LED_OFF"
      digitalWrite(13, LOW);  // Turn off the LED connected to pin 13
      Serial.println("Command " + receivedMessage + " is completed successfully");  // Send a success response after executing the command
    }
    else if (receivedMessage.length() > 0) {  // If the received message is not empty
      String reversedMessage = "";  // Create an empty string to store the reversed message

      // Loop to reverse the received message
      for (int i = receivedMessage.length() - 1; i >= 0; i--) {
        reversedMessage += receivedMessage[i];  // Add each character in reverse order to the reversedMessage string
      }

      Serial.println(reversedMessage);  // Send the reversed message back to the serial port
    }
  }
}
