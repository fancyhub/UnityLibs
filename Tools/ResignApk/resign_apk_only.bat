@echo off
cd /d %~dp0

echo Resign Apk


SET INPUT_APK=%1
:SET INPUT_APK=%CD%\test_0.0.1.0.apk


SET SUFFIX=%INPUT_APK:~-5,-1%

if not "%SUFFIX%"==".apk" ( 
	SET INPUT_APK="%INPUT_APK%"
)
 
SET OUTPUT_APK=%INPUT_APK:.apk=_new.apk%
echo input:%INPUT_APK%
echo output:%OUTPUT_APK%

call tool\resign_apk_only.bat  %INPUT_APK% %OUTPUT_APK%

pause



