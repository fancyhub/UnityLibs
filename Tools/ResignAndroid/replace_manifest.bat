@echo off
cd /d %~dp0

echo replace AndroidManifest.xml in apk
call node tool\tool.js -config config.json replaceManifest "%~1" "%~2"
pause
