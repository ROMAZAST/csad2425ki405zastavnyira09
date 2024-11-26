@echo off

set CLIENT_PATH=%~dp0..\client
set SERVER_PATH=%~dp0..\server
set SERVER_PORT=COM5
set BUILD_DIR=%~dp0..\build
set IS_GITHUB_ACTION=%GITHUB_ACTIONS%

if "%IS_GITHUB_ACTION%"=="true" (
    echo [STATUS] Running inside GitHub Actions.
    set IS_LOCAL=false
) else (
    echo [STATUS] Running on local machine.
    set IS_LOCAL=true
)

if "%IS_LOCAL%"=="true" (
    echo [CHECK] Verifying Arduino CLI presence...
    if not exist arduino-cli.exe (
        echo [ACTION] Arduino CLI not found. Initiating download...
        curl -fsSL https://downloads.arduino.cc/arduino-cli/arduino-cli_latest_Windows_64bit.zip -o arduino-cli.zip
        if %errorlevel% neq 0 (
            echo [ERROR] Unable to download Arduino CLI. Exiting.
            pause
            exit /b %errorlevel%
        )
        tar -xf arduino-cli.zip
        del arduino-cli.zip
        echo [SUCCESS] Arduino CLI installed.
    ) else (
        echo [FOUND] Arduino CLI is already installed.
    )
)

echo ===================================
echo [TASK] Compiling the client application...
if not exist "%CLIENT_PATH%\Tic_Tac_Toe_Client\Tic_Tac_Toe_Client.csproj" (
    echo [ERROR] Client project file missing: %CLIENT_PATH%\Tic_Tac_Toe_Client\Tic_Tac_Toe_Client.csproj
    pause
    exit /b 1
)

dotnet build "%CLIENT_PATH%\Tic_Tac_Toe_Client\Tic_Tac_Toe_Client.csproj" --configuration Release /p:Platform="x86" --output "%BUILD_DIR%"
if %errorlevel% neq 0 (
    echo [FAIL] Compilation of the client application failed.
    pause
    exit /b %errorlevel%
)
echo [DONE] Client application compiled successfully.

echo [TASK] Removing unnecessary files in the build directory...
for /r "%BUILD_DIR%" %%f in (*) do (
    if not "%%~nxf"=="Tic_Tac_Toe_Client.exe" del "%%f"
)

echo ===================================
echo [TASK] Compiling server code...
if not exist "%SERVER_PATH%" (
    echo [ERROR] Server code not found in path: %SERVER_PATH%
    pause
    exit /b 1
)

arduino-cli.exe compile -b arduino:avr:nano "%SERVER_PATH%"
if %errorlevel% neq 0 (
    echo [FAIL] Server code compilation failed.
    pause
    exit /b %errorlevel%
)
echo [DONE] Server code compiled successfully.

if "%IS_LOCAL%"=="true" (
    echo ===================================
    echo [TASK] Uploading server code to Arduino on %SERVER_PORT%...
    arduino-cli.exe upload -p %SERVER_PORT% --fqbn arduino:avr:nano "%SERVER_PATH%"
    if %errorlevel% neq 0 (
        echo [FAIL] Upload to Arduino failed.
        pause
        exit /b %errorlevel%
    )
    echo [SUCCESS] Server code uploaded successfully.
)

echo ===================================
if "%IS_LOCAL%"=="true" (
    echo [TASK] Cleaning up temporary Arduino CLI files...
    if exist arduino-cli.exe del arduino-cli.exe
    if exist LICENSE.txt del LICENSE.txt
    echo [DONE] Temporary files removed.
)

echo ===================================
echo [STATUS] All operations completed successfully.
pause
