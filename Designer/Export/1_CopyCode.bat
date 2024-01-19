@echo off
cd /d %~dp0


 
echo ========== copy client cs files ===================
xcopy /S /Y Output\Client\CS\*.cs ..\..\UnityLibs\Packages\com.github.fancyhub.unitylibs.table\Runtime\AutoGen
echo.

@rem echo ========== copy client Lua files ===================
@rem xcopy /S /Y Output\Client\Lua\*.lua ..\..\TestLoadLua\table\gen\
@rem echo.

@rem echo ========== copy client Cpp files ===================
@rem xcopy /S /Y Output\Client\Cpp\*.cpp ..\..\TestLoadCpp\gen\
@rem xcopy /S /Y Output\Client\Cpp\*.h ..\..\TestLoadCpp\gen\
@rem echo.

@rem echo ========== copy sever Go files ===================
@rem xcopy /S /Y Output\Server\Go\*.go ..\..\TestLoadGo\config\
@rem echo.

echo "All Done"
pause