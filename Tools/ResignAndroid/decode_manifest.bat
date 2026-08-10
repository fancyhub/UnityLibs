@echo off
cd /d %~dp0

echo decode AndroidManifest.xml from apk
call node tool\tool.js -config config.json decodeManifest "%~1" "%~2"
pause
