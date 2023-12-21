::@echo off
cd /d %~dp0

::SET NDKROOT=C:\Program Files\Unity\2022.3.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK
SET NDKROOT=C:\PROGRA~1\Unity\2022.3.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK

SET PATH=%PATH%;%NDKROOT%

call ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk NDK_APP_DST_DIR=../Runtime/Plugins/Android/$(TARGET_ARCH_ABI)

::timeout /t 2
rm -r obj
::xcopy .\libs\*.so ..\Runtime\Plugins\Android  /Y /S
pause