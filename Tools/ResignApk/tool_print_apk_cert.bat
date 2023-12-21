@echo off
cd /d %~dp0

SET APK_SIGNER=java -jar tool\apksigner.jar
%APK_SIGNER% verify -v --print-certs %1

pause