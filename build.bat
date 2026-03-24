@echo off
setlocal

:: Find MSBuild path using vswhere
set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %VSWHERE% (
    echo Error: vswhere.exe not found at %VSWHERE%
    echo Make sure Visual Studio Installer is installed.
    exit /b 1
)

echo Locating MSBuild...
for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
  set MSBUILD="%%i"
)

if not defined MSBUILD (
    echo Error: Could not find MSBuild.exe
    echo Make sure Visual Studio has the MSBuild component installed.
    exit /b 1
)

echo Using MSBuild at %MSBUILD%

:: Execute build for the solution
%MSBUILD% hibouengine.sln /p:Configuration=Debug /p:Platform=x64 -m

if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    exit /b %errorlevel%
)

echo.
echo Build succeeded!
