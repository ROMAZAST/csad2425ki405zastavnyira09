@echo off
setlocal

:: Отримати шлях до поточної директорії, де знаходиться скрипт
set SCRIPT_DIR=%~dp0

:: Піднятися на один рівень вгору, щоб виконати команду у батьківській директорії
cd /d "%SCRIPT_DIR%.."

:: Шлях до Doxyfile, який знаходиться в підкаталозі "доксіфайл"
set DOXYFILE=..\Doxyfile

:: Перевірка, чи існує файл Doxyfile
if not exist "Doxyfile" (
    echo Doxyfile not found at: SCRIPT_DIR
    exit /b 1
)

:: Запуск Doxygen з вказаним Doxyfile
echo Generating Doxygen documentation...
doxygen Doxyfile

:: Перевірка на помилки при виконанні Doxygen
if %errorlevel% neq 0 (
    echo Doxygen generation failed.
    exit /b %errorlevel%
)

echo Documentation generation completed successfully.

pause
pause
