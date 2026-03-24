@echo off
cd /d "%~dp0"

"Tools\Sharpmake\Sharpmake.Application\bin\Debug\net8.0\Sharpmake.Application.exe" "/sources('HibouEngine.sharpmake.cs')"

pause