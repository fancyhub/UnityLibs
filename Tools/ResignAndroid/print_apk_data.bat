@echo off
cd /d %~dp0

echo print apk data
SET INPUT_FILE=%1

call node tool\tool.js -config config.json printApkData "%INPUT_FILE%"
pause
