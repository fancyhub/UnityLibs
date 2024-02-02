::@echo off
set OldDir=%CD%
cd /d %~dp0
set CurDir=%CD%
 
SET INPUT_AAB=%1
::Apk out File path
set OUPUT_APKS=%2


if not exist %INPUT_AAB% (	
	echo can't find aab : %INPUT_AAB%
	goto end
)

call ../keystores/__set_keystore.bat
cd /D %CurDir%

SET BundleToolJar=bundletool-all-1.15.6.jar

SET BUILD_APKS_CMD=java -jar %BundleToolJar% build-apks
SET BUILD_APKS_CMD=%BUILD_APKS_CMD%  --mode=universal --bundle=%INPUT_AAB% --output=%OUPUT_APKS%
SET BUILD_APKS_CMD=%BUILD_APKS_CMD% --aapt2=aapt2.exe
SET BUILD_APKS_CMD=%BUILD_APKS_CMD% --ks=%KEYSTORE_FILE% --ks-pass=pass:%KEYSTORE_STOREPASS% --ks-key-alias=%KEYSTORE_ALIAS% --key-pass=pass:%KEYSTORE_KEYPASS%
SET BUILD_APKS_CMD=%BUILD_APKS_CMD% --overwrite
SET BUILD_APKS_CMD=%BUILD_APKS_CMD% --verbose
%BUILD_APKS_CMD%


if %errorlevel% == 0 (goto end) else (pause)

:end
cd /D %OldDir%