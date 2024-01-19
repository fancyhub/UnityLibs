@echo off
cd /d %~dp0

echo ===========================0.clear Output============================
del /F /S /Q Output\*.bin
del /F /S /Q Output\*.csv
del /F /S /Q Output\*.json
del /F /S /Q Output\*.bson
del /F /S /Q Output\*.cs
del /F /S /Q Output\*.go
del /F /S /Q Output\*.lua
del /F /S /Q Output\*.cpp
del /F /S /Q Output\*.h
echo.
echo.

echo ===========================1.gen data and code ===============
..\..\Tools\ExportExcel\ExportExcel.exe
echo.
echo.