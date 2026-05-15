::@echo off
cd /d %~dp0

copy WebView2UnityPlugin\x64\Release\WebView2UnityPlugin.dll ..\Runtime\Plugins\Windows\x64\ /Y
rd /s /q WebView2UnityPlugin\Release
rd /s /q WebView2UnityPlugin\Debug
rd /s /q WebView2UnityPlugin\x64

rd /s /q WebView2UnityPlugin\Demo\Release
rd /s /q WebView2UnityPlugin\Demo\Debug
rd /s /q WebView2UnityPlugin\Demo\x64

rd /s /q WebView2UnityPlugin\WebView2UnityPlugin\Release
rd /s /q WebView2UnityPlugin\WebView2UnityPlugin\Debug
rd /s /q WebView2UnityPlugin\WebView2UnityPlugin\x64

rd /s /q WebView2UnityPlugin\packages\


pause