@echo off
cd /d %~dp0

echo Resign Apk


SET INPUT_FILE=%1

call node tool\tool.js -config config.json resign "%INPUT_FILE%"

pause



