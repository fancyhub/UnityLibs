@echo off
cd /d %~dp0
set CurDir=%CD%

echo convert aab to apks

SET INPUT_FILE=%1

call node tool\tool.js -config config.json aab2apks "%INPUT_FILE%"


pause