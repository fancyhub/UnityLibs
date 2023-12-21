@echo off
cd /d %~dp0
set CurDir=%CD%

::SET INPUT_APK=%CD%\test_0.0.1.0.apk
::apk source file path
SET INPUT_APK=%1

::Res replacement dir
set REPLACE_RES_DIR=%2

::apk output file path
set OUPUT_APK=%3


if not exist %INPUT_APK% (
	echo can't find apk: %INPUT_APK% 
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
set APK_SIGNER_SIGN=%APK_SIGNER_SIGN% --v1-signing-enabled true --v2-signing-enabled false

echo %INPUT_APK%



:step_copy
echo= 
echo ===========Step 1 Copy apk=========  
echo %INPUT_APK% TO %UNSIGNED_APK%
if exist %UNSIGNED_APK% (
	del /F /Q %UNSIGNED_APK%
)
copy %INPUT_APK% %UNSIGNED_APK%



:step_replace
echo= 
echo ===========Step 2 Replace Apk Resources=========  
echo %UNSIGNED_APK%
cd /d %REPLACE_RES_DIR%
jar -uf %UNSIGNED_APK%   .\
cd /d %CurDir%

:step_zipalign
echo= 
echo ===========Step3 zip align========= 
echo %UNSIGNED_APK% TO %SIGNED_APK%
if exist %SIGNED_APK% (
	del /F /Q %SIGNED_APK%
)
%APK_ZIP_ALIGN%  4 %UNSIGNED_APK% %SIGNED_APK%
%APK_ZIP_ALIGN% -c 4 %SIGNED_APK%

:step_sign
echo= 
echo ===========Step4 Resign========= 
%APK_SIGNER_SIGN% %SIGNED_APK% 
@echo Resign Apk Done!



:step_pring_apk_cert
echo= 
echo =========Step5 Print Source Apk Cert===========
echo %INPUT_APK%
%APK_SIGNER% verify -v --print-certs %INPUT_APK%

echo= 
echo =========Step6 Print New Apk Cert===========
:keytool -printcert -jarfile %SIGNED_APK%
echo %SIGNED_APK%
%APK_SIGNER% verify -v --print-certs %SIGNED_APK%

if exist %OUPUT_APK% (
	del /F /Q %OUPUT_APK%
)
move %SIGNED_APK% %OUPUT_APK%

echo=
echo=
echo Operation Done %OUPUT_APK%
echo=

:end
@pause