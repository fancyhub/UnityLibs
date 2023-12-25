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


AAsset* fh_native_io_file_open(const char* file_path,int mode) {
    if(g_asset_mgr== NULL)
    {
        LOGE("fh_native_io_file_open g_asset_mgr is NULl");
        return NULL;
    }

    LOGD("fh_native_io_file_open g_asset_mgr is not null: %s",file_path);
    AAsset* ret= AAssetManager_open(g_asset_mgr,file_path,mode );
    if(ret == NULL)
        LOGE("fh_native_io_file_open Failed: %s", file_path);
    else
        LOGD("fh_native_io_file_open Succ: %p, %s",ret, file_path);
    return ret;
}

void fh_native_io_file_close(AAsset* fhandle)
{    
    LOGD("fh_native_io_file_close: %p",fhandle);
    if(fhandle == NULL)
    {
        LOGE("fh_native_io_file_close: handle is null");
        return;
    }
    AAsset_close(fhandle);
}

long long  fh_native_io_file_get_len(AAsset* fhandle)
{
    LOGD("fh_native_io_file_get_len : %p", fhandle);
    if(fhandle == NULL)
    {
        LOGE("fh_native_io_file_get_len: handle is null");
        return 0;
    }
        
    long long ret= AAsset_getLength64(fhandle);
    LOGD("fh_native_io_file_get_len : %p, %lld", fhandle,ret);
    return ret;
}

long long   fh_native_io_file_seek(AAsset* fhandle,long long  offset, int whence)
{
    if(fhandle==NULL)
    {
        LOGE("fh_native_io_file_seek: handle is null");
        return 0;
    }
    LOGD("fh_native_io_file_seek : %p, %lld,%d", fhandle,offset,whence);
    long long ret = AAsset_seek64(fhandle,offset,whence);
    LOGD("fh_native_io_file_seek : result %lld", ret);
    return ret;
}

int fh_native_io_file_read(AAsset* fhandle,unsigned char* buf,int offset, int count)
{
    LOGD("fh_native_io_file_read : %p, %p, %d,%d", fhandle,buf,offset,count);
    if(buf == NULL)
    {
        LOGE("fh_native_io_file_read : buf is NULl");
        return 0;
    }
    if(fhandle==NULL)
    {
        LOGE("fh_native_io_file_read : Handle is NULl");
        return 0;
    }
    if(offset<0)
    {
        LOGD("fh_native_io_file_read : offset <0, %d",offset);
        return 0;
    }

    if(count <=0)
    {
        LOGD("fh_native_io_file_read : count <=0, %d",count);
        return 0;
    }

    int ret= AAsset_read(fhandle,buf+offset,count);
    LOGD("fh_native_io_file_read : readed count %d", ret);
    return ret;
}

int fh_native_io_get_file_count()
{
    return (int)g_fh_file_list.size();
}

int fh_native_io_get_file(int index,unsigned char* buff,int buff_size)
{
    if(index<0 || index>=g_fh_file_list.size())
        return -1;
    
    if(buff==NULL)
    {
        LOGE("fh_native_io_get_file: buff is null");
        return -1;
    }

    const std::string& file_name =  g_fh_file_list[index];
    if(buff_size< file_name.size())
    {
        LOGE("fh_native_io_get_file: buff size is less than file name len , %d < %d, %s",buff_size,(int)file_name.size(),file_name.c_str());
        return -1;
    }

    memcpy(buff,file_name.c_str(),file_name.size());    
    return (int)file_name.size();
}
 
char* fh_native_io_malloc(int count)
{
    LOGD("fh_native_io_malloc : %d",count);
    auto ret= malloc(count);

    LOGD("fh_native_io_malloc : %p, %d ",ret,count);
    return static_cast<char *>(ret);
}

void fh_native_io_free(char* buf)
{
    LOGD("fh_native_io_free : %p ",buf);
    if(buf == NULL)
        return;
    free(buf);
}

#ifdef __cplusplus
};
#endif