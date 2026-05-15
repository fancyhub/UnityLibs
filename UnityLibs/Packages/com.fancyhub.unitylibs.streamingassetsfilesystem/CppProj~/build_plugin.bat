::@echo off
cd /d %~dp0

::SET NDKROOT=C:\PROGRA~1\Unity\2022.3.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK
SET NDKROOT=C:\tools\android_sdk\ndk\23.1.7779620

SET PATH=%PATH%;%NDKROOT%

call ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=Application.mk NDK_APP_DST_DIR=../Runtime/Plugins/Android/nativeio.androidlib/libs/$(TARGET_ARCH_ABI)

rd /s /q obj
del obj.meta

pause