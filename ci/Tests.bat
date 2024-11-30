@echo off
setlocal enabledelayedexpansion

if not "%cd%\"=="%~dp0" (
    echo [STATUS] Not in 'ci' directory, changing to ./ci...
    cd ./ci
)
REM Default values
set SERIAL_PORT=COM5
set BAUD_RATE=9600

REM Check if the script is running in GitHub Actions by checking if the GITHUB_ACTIONS environment variable is defined
if defined GITHUB_ACTIONS (
    echo [STATUS] Running inside GitHub Actions.
    set IS_GITHUB_ACTION=true
) else (
    echo [STATUS] Running on local machine.
    set IS_GITHUB_ACTION=false
)

REM Output the status of IS_GITHUB_ACTION for debugging
echo [DEBUG] IS_GITHUB_ACTION=!IS_GITHUB_ACTION!

REM If running locally, allow the user to provide SERIAL_PORT and BAUD_RATE
if "!IS_GITHUB_ACTION!"=="false" (
    echo [STATUS] Running locally - checking for command-line arguments...

    if not "%1"=="" (
        echo [DEBUG] Setting SERIAL_PORT to %1
	set SERIAL_PORT=%1
	if not "%2"=="" (
        	echo [DEBUG] Setting BAUD_RATE to %2
		set BAUD_RATE=%2
    	)
	echo [DEBUG] Updated SERIAL_PORT=!SERIAL_PORT!, BAUD_RATE=!BAUD_RATE!
        
    )

    REM Path to the test project for TicTacToeTests
    set TEST_PROJECT=..\client\TicTacToeTests\TicTacToeTests.csproj

    REM Run the tests with parameters and generate reports
    echo [STATUS] Running TicTacToeTests...
    dotnet test !TEST_PROJECT! --logger "trx;LogFileName=test_results.trx"
    echo [SUCCESS] TicTacToeTests completed successfully.
)

REM Run GUI_TTT_Tests for both local and GitHub Actions environments
echo ===================================
echo [STATUS] Running GUI_TTT_Tests...
set TEST_PROJECT=..\client\GUI_TTT_Tests\GUI_TTT_Tests.csproj
dotnet test %TEST_PROJECT% --logger "trx;LogFileName=test_results.trx"

REM Indicate the end of the process
echo [STATUS] All tests completed successfully.

REM Pause to keep the window open
pause
