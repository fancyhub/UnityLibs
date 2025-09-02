@echo off
cd /d %~dp0

echo replace resources in aab/apk 
SET INPUT_FILE=%1
set ASSET_DIR=%CD%\res_replacement\%ResName%

call node tool\tool.js -config config.json replaceRes "%INPUT_FILE%" "%ASSET_DIR%"

pause

