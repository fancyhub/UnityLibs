#include <jni.h>
#include <android/asset_manager.h>
#include <android/asset_manager_jni.h>
#include <unistd.h>
#include <memory.h>
#include <android/log.h>
#include <string.h>
#include <stdlib.h>

#define TAG    "<NativeIO>"

//#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG,TAG,__VA_ARGS__)
#define LOGI(...)  __android_log_print(ANDROID_LOG_INFO,TAG,__VA_ARGS__)
#define LOGW(...)  __android_log_print(ANDROID_LOG_WARN,TAG,__VA_ARGS__)
#define LOGE(...)  __android_log_print(ANDROID_LOG_ERROR,TAG,__VA_ARGS__)
#define LOGF(...)  __android_log_print(ANDROID_LOG_FATAL,TAG,__VA_ARGS__)
#define LOGD(...)


class MyString {
private:
    char* m_data;   
    size_t m_len;

    void free_buff() {
        if (m_data != nullptr) {
            delete[] m_data;
            m_data = nullptr;
            m_len = 0;
        }
    }

public:
    MyString() : m_data(nullptr), m_len(0) {
        m_data = new char[1];
        m_data[m_len] = '\0';
    }

    MyString(const char* cstr) : m_data(nullptr), m_len(0) {
        if (cstr == nullptr) {
            m_data = new char[1];
            m_data[0] = '\0';
        } else {
            m_len = strlen(cstr);
            m_data = new char[m_len + 1];  
            memcpy(m_data, cstr,m_len+1);
            m_data[m_len]='\0';
        }
    }

    MyString(const MyString& other) : m_data(nullptr), m_len(other.m_len) {
        m_data = new char[m_len + 1];
        memcpy(m_data, other.m_data,m_len+1);
        m_data[m_len]='\0';
    }

    ~MyString() {
        free_buff();
    }

    MyString& operator=(const MyString& other) {
        if (this != &other) {  
            free_buff();
            m_len = other.m_len;
            m_data = new char[m_len + 1];
            memcpy(m_data, other.m_data,m_len+1);
            m_data[m_len]='\0';
        }
        return *this;
    }

    size_t size()const{
        return m_len;
    }

    const char* c_str() const {
        return m_data;
    }
};


template <typename T>
class MyVector {
private:
    T* _data;
    size_t _size;
    size_t _capacity;

    void resize_capacity(size_t new_capacity) {
        if (new_capacity <= _capacity) {
            return;  
        }

        T* new_data = new T[new_capacity];
        for(int i=0;i<_size;i++)
        {
            new_data[i]=_data[i];
        }
        delete[] _data;
        _data = new_data;
        _capacity = new_capacity;
    }

public:
    explicit MyVector(size_t init_capacity = 100) 
        : _data(nullptr), _size(0), _capacity(0) {
        if (init_capacity > 0) {
            _data = new T[init_capacity];
            _capacity = init_capacity;
        }
    }

    ~MyVector() {
        delete[] _data;
        _data = nullptr;
        _size = 0;
        _capacity = 0;
    }

    void push_back(const T& elem) {
        if (_size >= _capacity) {
            resize_capacity(_capacity == 0 ? 4 : _capacity * 2);
        }
        _data[_size++] = elem;
    } 

    const T& operator[](size_t index) const {        
        return _data[index];
    }

    size_t size() const {
        return _size;
    }

    void clear() {
        _size = 0;
    }
};



static MyVector<MyString> g_fh_file_list;
 

#ifdef __cplusplus
extern "C" {
#endif
//https://developer.android.google.cn/ndk/reference/group/asset
static AAssetManager* g_asset_mgr=NULL;


struct MyAsset
{
	AAsset* AssetHandle;
	int FD;
	off64_t Start;
	off64_t Length;	
};

JNIEXPORT void JNICALL Java_com_fancyhub_nativeio_JNIContext_nativeSetContext(
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

JNIEXPORT void JNICALL Java_com_fancyhub_nativeio_JNIContext_nativeAddFolder(
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
        const char* file_name =  AAssetDir_getNextFileName(asset_dir);
        if(file_name ==NULL)
            break;

        auto file_name_len =strlen(file_name);
        if( file_name_len == 0)
        {
            LOGD("Add File Ignore: %s",file_name);
            continue;
        }
         
        size_t buff_size = strlen(folder_path) + file_name_len +2;
        if(buff_size<=1024)
        {
            char temp[1024];
            sprintf(temp,"%s/%s",folder_path,file_name);
            g_fh_file_list.push_back(temp);  
            LOGD("Add File: %s",temp);
        }
        else
        {
            char* temp = (char *)malloc(buff_size);
            sprintf(temp,"%s/%s",folder_path,file_name);
            g_fh_file_list.push_back(temp);  
            LOGD("Add File: %s",temp);
            free(temp);
        }
    }

    AAssetDir_close(asset_dir);
    env->ReleaseStringUTFChars(jstrFolderPath, folder_path);
}

MyAsset* fh_native_io_file_open(const char* file_path) {
    if(g_asset_mgr== NULL)
    {
        LOGE("fh_native_io_file_open g_asset_mgr is NULl");
        return NULL;
    }

    LOGD("fh_native_io_file_open g_asset_mgr is not null: %s",file_path);
    AAsset* asset= AAssetManager_open(g_asset_mgr,file_path,AASSET_MODE_STREAMING);
    if(asset == NULL)
	{	
        LOGE("fh_native_io_file_open Failed: %s", file_path);
		return NULL;
	}
	
	off64_t start,length;
	int fd= AAsset_openFileDescriptor64(asset,&start, &length);
	if(fd>=0)	
		lseek64(fd,start,SEEK_SET);	
	LOGD("fh_native_io_file_open Succ: AssetHandle: %p, FD:%d, Start:%lld, Length:%lld, Path:%s",asset, fd,(long long )start,(long long )length, file_path);
	
	MyAsset* ret = new MyAsset();
	ret->AssetHandle = asset;
	
	ret->FD= fd;
	ret->Start = start;
	ret->Length = length;
	
    return ret;
}

void fh_native_io_file_close(MyAsset* fhandle)
{    
    LOGD("fh_native_io_file_close: %p",fhandle);
    if(fhandle == NULL)
    {
        LOGE("fh_native_io_file_close: handle is null");
        return;
    }
	if(fhandle->FD>=0)
	{
		close(fhandle->FD);
		fhandle->FD=-1;
	}
	
	if(fhandle->AssetHandle!=NULL)
	{
		AAsset_close(fhandle->AssetHandle);
		fhandle->AssetHandle=NULL;
	}
	delete fhandle;	
}

int fh_native_io_file_get_pos(MyAsset* fhandle)
{
	LOGD("fh_native_io_file_get_pos : %p", fhandle);
    if(fhandle == NULL)
    {
        LOGE("fh_native_io_file_get_pos: handle is null");
        return -1;
    }
	
	if(fhandle->FD>=0)
	{
		long long nowPos =lseek64(fhandle->FD, 0, SEEK_CUR);
		long long ret= nowPos - fhandle->Start;
		LOGD("fh_native_io_file_get_pos : %p, FD:%d, %lld, nowPos:%lld, start:%lld", fhandle->AssetHandle,fhandle->FD,ret, nowPos,(long long)fhandle->Start);		
		return (int)ret;
	}
	else 
	{
		long long ret= AAsset_seek64(fhandle->AssetHandle,0, SEEK_CUR);
		LOGD("fh_native_io_file_get_pos : %p, %lld", fhandle->AssetHandle,ret);	
		return (int)ret;
	}
}

long long  fh_native_io_file_get_len(MyAsset* fhandle)
{
    LOGD("fh_native_io_file_get_len : %p", fhandle);
    if(fhandle == NULL)
    {
        LOGE("fh_native_io_file_get_len: handle is null");
        return 0;
    }
	
	if(fhandle->FD>=0)
	{
		return fhandle->Length;
	}
        
    long long ret= AAsset_getLength64(fhandle->AssetHandle);
    LOGD("fh_native_io_file_get_len : %p, %lld", fhandle,ret);
    return ret;
}

//ret current pos
long long fh_native_io_file_seek(MyAsset* fhandle, long long  offset, int whence)
{
    if(fhandle==NULL)
    {
        LOGE("fh_native_io_file_seek: handle is null");
        return 0;
    }
    LOGD("fh_native_io_file_seek : %p, %lld,%d", fhandle,offset,whence);
	
	long long ret=0;
	if(fhandle->FD<0)
	{
		ret= AAsset_seek64(fhandle->AssetHandle,offset,whence);
		LOGD("fh_native_io_file_seek : result %lld", ret);
		return ret;
	}
	
	long long newRelativePos=0;
	
	switch(whence)
	{
		case SEEK_SET:
			newRelativePos = offset;
			break;
			
		case SEEK_CUR:
			 newRelativePos= lseek64(fhandle->FD, 0, SEEK_CUR) - fhandle->Start + offset;
			break;
			
		case SEEK_END:
			newRelativePos = fhandle->Length + offset;
			break;
	}

	if(newRelativePos <0 || newRelativePos > fhandle->Length)
		return -1;

	long long newPos= fhandle->Start + newRelativePos;
	long long temp = lseek64(fhandle->FD, newPos, SEEK_SET);
	if(temp<0)
		return -1;
	return temp - fhandle->Start;
}


bool fh_native_io_file_can_seek(MyAsset* fhandle)
{
    if(fhandle==NULL)
    {
        LOGE("fh_native_io_file_can_seek: handle is null");
        return false;
    }
	return fhandle->FD>=0;
}

int fh_native_io_file_read(MyAsset* fhandle,unsigned char* buf,int offset, int count)
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
	
	if(fhandle->FD<0)
	{
		int ret= AAsset_read(fhandle->AssetHandle,buf+offset,count);
		LOGD("fh_native_io_file_read : readed count %d", ret);
		return ret;
	}
	else
	{
		long long nowPos = lseek64(fhandle->FD, 0, SEEK_CUR);		
		int  remainCount = (int)(fhandle->Length - (nowPos - fhandle->Start));		
		int toReadCount = remainCount>count? count:remainCount;
		int ret = read(fhandle->FD,buf+offset,toReadCount);			
		LOGD("fh_native_io_file_read : readed count %d", ret);
		return ret;
	}    
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

    const MyString& file_name =  g_fh_file_list[index];
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