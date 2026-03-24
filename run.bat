@echo off
setlocal

set EXE_PATH="Editor\output\win64\debug\net10.0-windows\Editor.exe"

if not exist %EXE_PATH% (
    echo Error: Editor.exe not found at %EXE_PATH%
    echo Please run build.bat first to build the project.
    exit /b 1
)

echo Launching HibouEngine Editor...
start "" %EXE_PATH%
