@echo off
cd /d %~dp0

SET ResName=sample
echo Resign With [%ResName%]

::SET INPUT_APK=%CD%\test_0.0.1.0.apk
SET INPUT_APK="%1"

set ASSET_DIR=%CD%\res_replacement\%ResName%
set OUTPUT_APK=%INPUT_APK:~0,-5%_%ResName%.apk"

call tool\resign_apk.bat  %INPUT_APK%  %ASSET_DIR%  %OUTPUT_APK%



