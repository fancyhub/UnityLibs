echo off
cd /d %~dp0
set CurDir=%CD%

echo =========1. generate temp universal apks=========
SET INPUT_AAB=%1
SET Output_Apks=%CurDir%\temp\temp.apks
tool\aab_2_apks.bat %INPUT_AAB% %Output_Apks%

echo =======2.extra universal apk ================
cd temp
jar -xf temp.apks universal.apk
cd /D %CurDir%

echo =========3.install temp universal  apk=========
adb install temp\universal.apk
::java -jar tool\bundletool-all-1.15.6.jar install-apks --apks=%Output_Apks% 

pause



