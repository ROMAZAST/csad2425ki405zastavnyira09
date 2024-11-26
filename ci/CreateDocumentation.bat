@echo off
setlocal

set SCRIPT_DIR=%~dp0

cd /d "%SCRIPT_DIR%.."

:: Перевірка, чи існує файл Doxyfile
if not exist "Doxyfile" (
    echo Doxyfile not found at: SCRIPT_DIR
    exit /b 1
)

echo Generating Doxygen documentation...
doxygen Doxyfile

if %errorlevel% neq 0 (
    echo Doxygen generation failed.
    exit /b %errorlevel%
)

echo Documentation generation completed successfully.

pause
