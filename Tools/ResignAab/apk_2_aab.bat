::@echo off
cd /d %~dp0
set CurDir=%CD%
echo on
::goto step3

echo ==========Clear temp dir=========
if exist temp ( 
    del /F /Q /S temp\*
    rd /s /q temp
)
::mkdir temp


echo ====Unzip apk========
SET Input_Apk=E:\fancyHub\UnityLibs\Tools\ResignAab\temp1\KD_android_RCT_804106_0.0.11.apk
java -jar tool\apktool_2.9.3.jar d %Input_Apk% -s -o  temp


:step3
echo =========== Compile resources=================
SET AAPT2_EXE=%CurDir%\tool\aapt2.exe
%AAPT2_EXE% compile --dir temp\res -o compiled_resources.zip

cd /D %CurDir%