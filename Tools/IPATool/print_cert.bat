@echo off
cd /d %~dp0

echo print cert
SET INPUT_FILE=%1

call node tool\tool.js printCert %INPUT_FILE%
pause