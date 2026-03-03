@echo off
cd /d %~dp0

echo convert plist
SET INPUT_FILE=%1

call node tool\tool.js convertPlist %INPUT_FILE%
pause