::@echo off
cd /d %~dp0

::SET NDKROOT=C:\PROGRA~1\Unity\2022.3.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK
SET NDKROOT=C:\tools\android_sdk\ndk\23.1.7779620

SET PATH=%PATH%;%NDKROOT%

call ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk NDK_APP_DST_DIR=../Runtime/Plugins/Android/$(TARGET_ARCH_ABI)

::timeout /t 2
rmdir /s obj
del obj.meta
:: rm -r ../Runtime/Plugins/Android/*/libc++_shared.so
::xcopy .\libs\*.so ..\Runtime\Plugins\Android  /Y /S
pause