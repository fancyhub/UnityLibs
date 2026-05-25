@echo off
setlocal
cd /d "%~dp0"
dotnet build -c Debug
if errorlevel 1 exit /b %errorlevel%
start "" "%~dp0bin\Debug\net48\NativeLibTool.exe"
