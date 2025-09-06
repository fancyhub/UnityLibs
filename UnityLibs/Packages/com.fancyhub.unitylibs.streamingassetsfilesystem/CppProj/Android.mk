LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)

LOCAL_MODULE := fhnativeio          
LOCAL_SRC_FILES := SAFileSystem.cpp  
LOCAL_LDLIBS := -llog -landroid      

LOCAL_CFLAGS := -DANDROID_NDK -O2    
LOCAL_CPPFLAGS := -std=c++11 -O2 -fno-exceptions -fno-rtti  

LOCAL_LDFLAGS += -Wl,-z,max-page-size=16384  
LOCAL_LDFLAGS += -Wl,-z,common-page-size=16384
LOCAL_LDFLAGS += -s   
LOCAL_LDFLAGS += -Wl,--gc-sections 

include $(BUILD_SHARED_LIBRARY)