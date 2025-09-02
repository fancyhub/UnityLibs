@echo off
cd /d %~dp0

SET INPUT_FILE=%1

call node tool\tool.js -config config.json printCertSelf
pause