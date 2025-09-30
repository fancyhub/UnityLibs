#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <ole2.h>               // 提供COM基础类型
#include <ocidl.h>              // 提供IStream等接
#include <WebView2.h>
#include <wrl.h>
using namespace Microsoft::WRL;