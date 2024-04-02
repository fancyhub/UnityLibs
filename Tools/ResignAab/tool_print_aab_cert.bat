@echo off
cd /d %~dp0

java -jar tool\bundletool-all-1.15.6.jar dump manifest --bundle %1
::keytool -printcert -jarfile %1
::jarsigner -verify  -verbose %1
pause