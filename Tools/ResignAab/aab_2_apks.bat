echo off
cd /d %~dp0
set CurDir=%CD%

echo Aab To Apk

SET INPUT_AAB=%1
::SET INPUT_AAB=%CD%\temp\test.aab


SET SUFFIX=%INPUT_AAB:~-5,-1%

if not "%SUFFIX%"==".aab" ( 
	SET INPUT_AAB="%INPUT_AAB%"
)


SET OUTPUT_APK=%INPUT_AAB:.aab=.apks%
echo input:%INPUT_AAB%
echo output:%OUTPUT_APK%

call tool\aab_2_apks.bat %INPUT_AAB% %OUTPUT_APK%

pause



