@echo off
setlocal
cd /d "%~dp0"
dotnet build -c Release
