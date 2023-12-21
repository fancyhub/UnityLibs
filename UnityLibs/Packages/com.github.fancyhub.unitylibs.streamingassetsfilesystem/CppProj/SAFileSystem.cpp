#include <jni.h>
#include <string>
#include <vector>
#include <android/asset_manager.h>
#include <android/asset_manager_jni.h>
#include <memory.h>
#include <android/log.h>

#define TAG    "<NativeIO>"

//#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG,TAG,__VA_ARGS__)
#define LOGI(...)  __android_log_print(ANDROID_LOG_INFO,TAG,__VA_ARGS__)
#define LOGW(...)  __android_log_print(ANDROID_LOG_WARN,TAG,__VA_ARGS__)
#define LOGE(...)  __android_log_print(ANDROID_LOG_ERROR,TAG,__VA_ARGS__)
#define LOGF(...)  __android_log_print(ANDROID_LOG_FATAL,TAG,__VA_ARGS__)
#define LOGD(...)

//https://developer.android.google.cn/ndk/reference/group/asset
static std::vector<std::string> g_fh_file_list;


 

#ifdef __cplusplus
extern "C" {
#endif

static AAssetManager* g_asset_mgr=NULL;


JNIEXPORT void JNICALL Java_com_github_fancyhub_nativeio_JNIContext_nativeSetContext(
        JNIEnv* env,
        jobject /*this*/ inst,
        jobject assetManager)
{
    g_fh_file_list.clear();
    g_asset_mgr=AAssetManager_fromJava(env, assetManager);
    if(g_asset_mgr!= NULL)
        LOGD("nativeSetContext succ");
    else
        LOGD("nativeSetContext failed");    
}

JNIEXPORT void JNICALL Java_com_github_fancyhub_nativeio_JNIContext_nativeAddFolder(
        JNIEnv* env,
        jobject /*this*/ inst,
        jstring jstrFolderPath)
{
    if(g_asset_mgr== NULL)
    {
        LOGE("nativeAddFolder asset_mgr is NULL");
        return;
    }

    jboolean isCopy=false;
    const char* folder_path = env->GetStringUTFChars(jstrFolderPath,&isCopy);
    LOGD("Add Folder: %s",folder_path);

    if(folder_path == NULL)
        return;
    AAssetDir* asset_dir= AAssetManager_openDir(g_asset_mgr,folder_path);
    if(asset_dir == NULL)
    {
        env->ReleaseStringUTFChars(jstrFolderPath, folder_path);
        LOGE("asset_dir is NULl");
        return;
    }
    for(;;)
    {
        const char* ret =  AAssetDir_getNextFileName(asset_dir);
        if(ret ==NULL)
            break;

        if(strlen(ret) == 0)
        {            
            g_fh_file_list.push_back(ret);
            LOGD("Add File: %s",ret);
        }
        else 
        {
            char buff[1024];
            sprintf(buff,"%s/%s",folder_path,ret);
            const char* const_buff = buff;
            g_fh_file_list.push_back(const_buff);
            LOGD("Add File: %s",buff);
        }
        
    }

    AAssetDir_close(asset_dir);
    env->ReleaseStringUTFChars(jstrFolderPath, folder_path);
}


AAsset* native_io_file_open(const char* file_path) {
    if(g_asset_mgr== NULL)
    {
        LOGE("native_io_file_open g_asset_mgr is NULl");
        return NULL;
    }

    LOGD("native_io_file_open g_asset_mgr is not null: %s",file_path);
    AAsset* ret= AAssetManager_open(g_asset_mgr,file_path,AASSET_MODE_STREAMING );
    if(ret == NULL)
        LOGE("native_io_file_open Failed: %s", file_path);
    else
        LOGD("native_io_file_open Succ: %p, %s",ret, file_path);
    return ret;
}

void native_io_file_close(AAsset* fhandle)
{    
    LOGD("native_io_file_close: %p",fhandle);
    if(fhandle == NULL)
    {
        LOGE("native_io_file_close: handle is null");
        return;
    }
    AAsset_close(fhandle);
}

long long  native_io_file_get_len(AAsset* fhandle)
{
    LOGD("native_io_file_get_len : %p", fhandle);
    if(fhandle == NULL)
    {
        LOGE("native_io_file_get_len: handle is null");
        return 0;
    }
        
    long long ret= AAsset_getLength64(fhandle);
    LOGD("native_io_file_get_len : %p, %lld", fhandle,ret);
    return ret;
}

long long   native_io_file_seek(AAsset* fhandle,long long  offset, int whence)
{
    if(fhandle==NULL)
    {
        LOGE("native_io_file_seek: handle is null");
        return 0;
    }
    LOGD("native_io_file_seek : %p, %lld,%d", fhandle,offset,whence);
    long long ret = AAsset_seek64(fhandle,offset,whence);
    LOGD("native_io_file_seek : result %lld", ret);
    return ret;
}

int native_io_file_read(AAsset* fhandle,char* buf,int len)
{
    LOGD("native_io_file_read : %p, %p, %d", fhandle,buf,len);
    if(buf == NULL)
    {
        LOGE("native_io_file_read : buf is NULl");
        return 0;
    }
    if(fhandle==NULL)
    {
        LOGE("native_io_file_read : Handle is NULl");
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

int native_io_get_file_count()
{
    return (int)g_fh_file_list.size();
}

const char* native_io_get_file(int index)
{
    if(index<0 || index>=g_fh_file_list.size())
        return NULL;
    return g_fh_file_list[index].c_str();
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