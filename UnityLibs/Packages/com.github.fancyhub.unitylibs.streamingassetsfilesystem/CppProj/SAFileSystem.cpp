#include <jni.h>
#include <string>
#include <android/asset_manager.h>
#include <android/asset_manager_jni.h>
#include <memory.h>
#include <android/log.h>

#define TAG    "NativeIO"
//#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG,TAG,__VA_ARGS__)
#define LOGD(...)

#ifdef __cplusplus
extern "C" {
#endif

static AAssetManager* g_asset_mgr=NULL;


JNIEXPORT void JNICALL Java_com_github_fancyhub_nativeio_JNIContext_nativeSetContext(
        JNIEnv* env,
        jobject /*this*/ inst,
        jobject assetManager)
{
    g_asset_mgr=AAssetManager_fromJava(env, assetManager);

    if(g_asset_mgr!= NULL)
        LOGD("JNIContext_nativeSetContext succ");
    else
        LOGD("JNIContext_nativeSetContext failed");
}


AAsset* native_io_file_open(const char* file_path) {
    if(g_asset_mgr== NULL)
    {
        LOGD(" native_io_file_open g_asset_mgr is NULl");
        return NULL;
    }

    LOGD(" native_io_file_open g_asset_mgr is not null: %s",file_path);
    AAsset* ret= AAssetManager_open(g_asset_mgr,file_path,AASSET_MODE_STREAMING );
    if(ret == NULL)
        LOGD("native_io_file_open Failed: %s", file_path);
    else
        LOGD("native_io_file_open Succ: %p, %s",ret, file_path);
    return ret;
}

void native_io_file_close(AAsset* fhandle)
{
    LOGD("native_io_file_close: %p",fhandle);
    if(fhandle == NULL)
        return;
    AAsset_close(fhandle);
}

long long  native_io_file_get_len(AAsset* fhandle)
{
    LOGD("native_io_file_get_len : %p", fhandle);
    if(fhandle == NULL)
        return 0;
    long long ret= AAsset_getLength64(fhandle);
    LOGD("native_io_file_get_len : %p, %d", fhandle,ret);
    return ret;
}

long long   native_io_file_seek(AAsset* fhandle,long long  offset, int whence)
{
    LOGD("native_io_file_seek : %p, %d,%d", fhandle,offset,whence);
    long long ret = AAsset_seek64(fhandle,offset,whence);
    LOGD("native_io_file_seek : result %d", ret);
    return ret;
}

int native_io_file_read(AAsset* fhandle,char* buf,int len)
{
    LOGD("native_io_file_read : %p, %p, %d", fhandle,buf,len);
    if(buf == NULL)
    {
        LOGD("native_io_file_read : buf is NULl");
        return 0;
    }

    if(len <=0)
    {
        LOGD("native_io_file_read : len <=0, %d",len);
        return 0;
    }

    int ret= AAsset_read(fhandle,buf,len);
    LOGD("native_io_file_read : readed count %d", ret);
    return ret;
}

AAssetDir* native_io_dir_open(const char* dir_path)
{
    if(g_asset_mgr== NULL )
    {
        LOGD("native_io_dir_open: g_asset_mgr is null");
        return NULL;
    }
    if(dir_path == NULL)
    {
        LOGD("native_io_dir_open: dir_path is null");
        return NULL;
    }

    LOGD("native_io_dir_open: %s",dir_path);
    AAssetDir* ret= AAssetManager_openDir(g_asset_mgr,dir_path);
    if(ret == NULL)
        LOGD("native_io_dir_open Failed: %s", dir_path);
    else
        LOGD("native_io_dir_open Succ: %p, %s",ret, dir_path);
    return ret;
}

void native_io_dir_close(AAssetDir* fhandle)
{
    LOGD("native_io_dir_close : %p",fhandle);

    if(fhandle == NULL)
        return;
    AAssetDir_close(fhandle);
}

const char* native_io_dir_next_file(AAssetDir* fhandle)
{
    LOGD("native_io_dir_next_file : %p",fhandle);
    if(fhandle == NULL)
    {
        LOGD("native_io_dir_next_file fhandle is null");
        return NULL;
    }

    const char* ret =  AAssetDir_getNextFileName(fhandle);
    if(ret != NULL)
        LOGD("native_io_dir_next_file: %s",ret);
    else
        LOGD("native_io_dir_next_file: NULL");
    return ret;
}

char* native_io_malloc(int count)
{
    LOGD("native_io_malloc : %d",count);
    auto ret= malloc(count);

    LOGD("native_io_malloc : %p, %d ",ret,count);
    return static_cast<char *>(ret);
}

void native_io_free(char* buf)
{
    LOGD("native_io_free : %p ",buf);
    if(buf == NULL)
        return;
    free(buf);
}

#ifdef __cplusplus
};
#endif