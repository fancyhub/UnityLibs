@echo off

CertUtil -hashfile "%1" MD5

echo=
pause