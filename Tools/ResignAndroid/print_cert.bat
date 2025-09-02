@echo off
cd /d %~dp0

echo print cert
SET INPUT_FILE=%1

call node tool\tool.js -config config.json printCert "%INPUT_FILE%"
pause