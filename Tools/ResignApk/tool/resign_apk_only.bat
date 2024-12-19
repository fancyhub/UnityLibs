::@echo off
cd /d %~dp0
set CurDir=%CD%

::SET INPUT_APK=%CD%\test_0.0.1.0.apk
::APK Source File Path
SET INPUT_APK=%1
::Apk out File path
set OUPUT_APK=%2


if not exist %INPUT_APK% (	
	echo can't find apk £º %INPUT_APK%
	goto end
)

call ../keystores/_set_keystore.bat
cd /d %CurDir%

SET UNSIGNED_APK=%CurDir%\..\output\0_unsinged.apk
SET SIGNED_APK=%CurDir%\..\output\1_signed.apk

set APK_ZIP_ALIGN=zipalign.exe
set APK_SIGNER=java -jar apksigner.jar

set APK_SIGNER_SIGN=%APK_SIGNER% sign     
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% --ks %KEYSTORE_FILE%  --ks-pass pass:%KEYSTORE_STOREPASS% 
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% --ks-key-alias %KEYSTORE_ALIAS% --key-pass pass:%KEYSTORE_KEYPASS%
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% -verbose 
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% --min-sdk-version 22
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% --v1-signing-enabled true --v2-signing-enabled true

echo %INPUT_APK%

:step_copy
echo= 
echo ===========Step1 Copy Apk=========  
echo %INPUT_APK% TO %OUPUT_APK%
if exist %OUPUT_APK% (
	del /F /Q %OUPUT_APK%
)
copy %INPUT_APK% %OUPUT_APK%
 
:step_sign
echo= 
echo ===========Step2 Resign========= 
%APK_SIGNER_SIGN% %OUPUT_APK% 
@echo Resign Apk Done!



:step_pring_apk_cert
echo= 
echo =========Step3 Print Source Apk cert===========
echo %INPUT_APK%
%APK_SIGNER% verify -v --print-certs %INPUT_APK%

echo= 
echo =========Step4 Print New Apk Cert===========
:keytool -printcert -jarfile %SIGNED_APK%
echo %SIGNED_APK%
%APK_SIGNER% verify -v --print-certs %OUPUT_APK%

echo=
echo=
echo Operation Done! %OUPUT_APK%
echo=

:end
pause