LOCAL_PATH :=$(call my-dir)
include $(CLEAR_VARS)

LOCAL_MODULE :=fhnativeio
LOCAL_SRC_FILES :=SAFileSystem.cpp
LOCAL_LDLIBS :=-llog -landroid
LOCAL_CFLAGS :=-DANROID_NDK
LOCAL_CPPFLAGS :=-std=c++11

include $(BUILD_SHARED_LIBRARY)