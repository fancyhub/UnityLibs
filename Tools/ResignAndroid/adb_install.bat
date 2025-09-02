@echo off
cd /d %~dp0
set CurDir=%CD%

echo install

SET INPUT_FILE=%1

call node tool\tool.js -config config.json install "%INPUT_FILE%"

pause