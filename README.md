## Repository Details
This repository contains a project for a Tic-Tac-Toe game created using Arduino for the CSAD course.

## Task Details
The main task is to develop a Tic-Tac-Toe game with the following features:

### Menu Options
- **Play Modes**:
  - **Man vs AI**
  - **Man vs Man**
  - **AI vs AI**
    - **Random Move**
    - **Win Strategy**
## Student and Task Details

| **Student Number** | **Game**            | **Configuration Format** |
|---------------------|---------------------|--------------------------|
| 9                   | Tic-Tac-Toe 3x3     | INI                      |

Let me know if you need any more changes!
## Technology and Hardware
- Hardware: Arduino
- Software: Windows Forms App
- Programming language: C#

# Task2: Creating a simple communication scheme between Microsoft Forms(client) and Arduino(server)

## How to Build the Project

### How to build the Server:
1. Download and install the Arduino IDE: [Arduino IDE Getting Started](https://docs.arduino.cc/tutorials/nano/nano-getting-started/)
2. Open the file located at `/Server/arduino_server.ino` using the Arduino IDE.
3. Select the correct COM port and Arduino board:
   - Go to **Tools** > **Port** and choose the appropriate port from the list.
   - Go to **Tools > Board** and choose the Arduino NANO.
4. Upload the program code to the Arduino board:
   - Click the **Upload** button (right arrow icon) in the Arduino IDE. Wait for the upload to complete; you should see a "Done uploading" message in the IDE status bar.

### How to build the Client:
1. **Open Microsoft Visual Studio:**
   - Launch Microsoft Visual Studio 

2. **Open the Solution File:**
   - Click on **File** in the top menu.
   - Select **Open** > **Project/Solution...**.
   - Navigate to the directory where your Tic Tac Toe client is located.
   - Find and select the `Tic_Tac_Toe_Client.sln` file and click **Open**.

4. **Build the Project:**
   - Click on **Build** in the top menu.
   - Select **Build Solution** (or simply press **Ctrl + Shift + B**).
