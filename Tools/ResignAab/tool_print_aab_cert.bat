@echo off

::keytool -printcert -jarfile %1
jarsigner -verify  -verbose %1
pause