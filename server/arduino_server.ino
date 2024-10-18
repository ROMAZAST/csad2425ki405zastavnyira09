const int BAUD_RATE = 9600;  // Define a constant for the baud rate

void setup() {
  Serial.begin(BAUD_RATE);  // Initialize the serial port 
}

void loop() {
  if (Serial.available() > 0) {
    String receivedMessage = Serial.readStringUntil('\n');  // Read the incoming message

    if (receivedMessage == "Arduino?") {
      Serial.println("Yes");  // Respond to the query
    }

    if (receivedMessage == "LED_ON") {
      digitalWrite(13, HIGH);  // Turn on the LED
      Serial.println("Command " + receivedMessage + " is completed successfully");  // Send a response after execution
    } 
    else if (receivedMessage == "LED_OFF") {
      digitalWrite(13, LOW);  // Turn off the LED
      Serial.println("Command " + receivedMessage + " is completed successfully");  // Send a response after turning off
    }
  }
}
