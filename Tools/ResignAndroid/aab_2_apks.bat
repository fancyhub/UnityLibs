echo off
cd /d %~dp0
set CurDir=%CD%

echo Aab To Apks

SET INPUT_FILE=%1

call node tool\tool.js -config config.json aab2apks "%INPUT_FILE%"


pause