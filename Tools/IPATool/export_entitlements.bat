@echo off
cd /d %~dp0

echo export entitlements
SET INPUT_FILE=%1

call node tool\tool.js exportEntitlements %INPUT_FILE%
pause
