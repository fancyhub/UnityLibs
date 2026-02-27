/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

#import <UIKit/UIKit.h>
#import <Photos/Photos.h>
#import <Foundation/Foundation.h>
#include <stdlib.h>

typedef void (*FHPhotoDataCallback)(const unsigned char* dataPtr, int dataLength, const char* assetID);

// 内部辅助类
@interface FHPhotoLibraryHelper : NSObject
+ (const char*)getLatestPhotoAssetIDSynchronous;
+ (void)loadImageDataForAssetID:(NSString *)assetID callback:(FHPhotoDataCallback)callback
@end

 

static id _notificationObserver = nil;

extern "C" {

typedef void (*FHScreenshotEventCallback)();
static FHScreenshotEventCallback s_ScreenshotEventCallback = NULL;

void FHStartScreenShotListener(FHScreenshotEventCallback callBack) 
{
    s_ScreenshotEventCallback=callBack;

    if (_notificationObserver) 
    {
        return;
    }
    
    _notificationObserver = [[NSNotificationCenter defaultCenter] addObserverForName:UIApplicationUserDidTakeScreenshotNotification 
                                                                                  object:nil 
                                                                                   queue:nil 
                                                                              usingBlock:^(NSNotification * _Nonnull note) {            
            // 【关键】如果回调指针有效，则调用它
            if (s_ScreenshotEventCallback != NULL) {                
                s_ScreenshotEventCallback();
            } else {
                NSLog(@"[ScreenShotListener] 警告：回调指针为空，无法通知 Unity");
            }
        }];
    }
}

void FHStopScreenShotListener() 
{
    s_ScreenshotEventCallback=NULL;

    if (_notificationObserver) {
        [[NSNotificationCenter defaultCenter] removeObserver:_notificationObserver];
        _notificationObserver = nil;
        NSLog(@"[ScreenShotListener] 监听已停止");
    }
}

const char* FHGetLatestPhotoAssetID() 
{
    return [FHPhotoLibraryHelper getLatestPhotoAssetIDSynchronous];
}

void FHLoadPhotoByAssetID(const char* cAssetID, FHPhotoDataCallback callBack) 
{
    NSString *assetID = [NSString stringWithUTF8String:cAssetID];
    [FHPhotoLibraryHelper loadImageDataForAssetID:assetID callback:callback];
}

// 保存图片到相册（返回值：0=成功，1=无权限，2=未知错误）
int FHSaveImageToPhotoAlbum(const char* imagePath) {
    // 1. 空路径直接返回未知错误
    if (imagePath == NULL || strlen(imagePath) == 0) {
        NSLog(@"保存失败：图片路径为空");
        return 2;
    }
    
    NSString *path = [NSString stringWithUTF8String:imagePath];
    
    // 2. 检查相册权限（无权限返回1）
    PHAuthorizationStatus status;
    if (@available(iOS 14, *)) {
        status = [PHPhotoLibrary authorizationStatusForAccessLevel:PHAccessLevelAddOnly];
    } else {
        status = [PHPhotoLibrary authorizationStatus];
    }
    
    // 仅Authorized（全版本）/Limited（iOS14+）判定为有权限
    BOOL hasPermission = (status == PHAuthorizationStatusAuthorized) || 
                         (status == PHAuthorizationStatusLimited && @available(iOS 14, *));
    if (!hasPermission) {
        NSLog(@"保存失败：无相册写入权限，当前状态：%d", status);
        return 1;
    }
    
    // 3. 检查文件是否存在（不存在返回2）
    if (![[NSFileManager defaultManager] fileExistsAtPath:path]) {
        NSLog(@"保存失败：图片文件不存在 - %@", path);
        return 2;
    }
    
    // 4. 同步写入相册
    __block int result = 0; // 默认成功
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    
    [[PHPhotoLibrary sharedPhotoLibrary] performChangesAndWait:^{
        // 创建写入请求
        PHAssetCreationRequest *request = [PHAssetCreationRequest creationRequestForAssetFromImageAtFileURL:[NSURL fileURLWithPath:path]];
        if (!request) {
            result = 2; // 请求创建失败，返回未知错误
        }
    } error:^(NSError * _Nullable error) {
        if (error) {
            NSLog(@"保存失败：写入相册出错 - %@", error.localizedDescription);
            result = 2;
        }
        dispatch_semaphore_signal(semaphore);
    }];
    
    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    return result;
}

}



@implementation FHPhotoLibraryHelper


+ (const char*)getLatestPhotoAssetIDSynchronous {
    PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
    if (status != PHAuthorizationStatusAuthorized) {
        return NULL;
    }

    PHFetchOptions *options = [[PHFetchOptions alloc] init];
    options.sortDescriptors = @[[NSSortDescriptor sortDescriptorWithKey:@"creationDate" ascending:NO]];
    options.fetchLimit = 1;
    options.predicate = [NSPredicate predicateWithFormat:@"mediaType == %d", PHAssetMediaTypeImage];

    PHFetchResult<PHAsset *> *result = [PHAsset fetchAssetsWithOptions:options];
    
    if (result.count > 0) {
        PHAsset *asset = [result firstObject];
        NSString *assetID = asset.localIdentifier;
        
         
        return strdup([assetID UTF8String]);        
    }
    return NULL;
}

+ (void)loadImageDataForAssetID:(NSString *)assetID callback:(FHPhotoDataCallback)callback {
    if (!assetID) {
        // 即使是错误情况，也建议抛回主线程处理，保持行为一致
        if (callback) {
            dispatch_async(dispatch_get_main_queue(), ^{
                callback(NULL, 0, NULL);
            });
        }
        return;
    }

    PHFetchResult<PHAsset *> *result = [PHAsset fetchAssetsWithLocalIdentifiers:@[assetID] options:nil];
    if (result.count == 0) {
        if (callback) {
            dispatch_async(dispatch_get_main_queue(), ^{
                callback(NULL, 0, [assetID UTF8String]);
            });
        }
        return;
    }

    PHAsset *asset = [result firstObject];
    
    PHImageRequestOptions *imageOptions = [[PHImageRequestOptions alloc] init];
    imageOptions.synchronous = NO;
    imageOptions.deliveryMode = PHImageRequestOptionsDeliveryModeHighQualityFormat;
    imageOptions.networkAccessAllowed = NO; // 禁用 iCloud

    NSLog(@"[PhotoLib] 请求本地图片数据: %@", assetID);

    [[PHImageManager defaultManager] requestImageDataForAsset:asset
                                                       options:imageOptions
                                                 resultHandler:^(NSData *imageData, NSString *dataUTI, UIImageOrientation orientation, NSDictionary *info) {
        
        unsigned char *nativeBuffer =NULL;
        if (imageData && callback)
        {
            NSUInteger length = [imageData length];
            const void *bytes = [imageData bytes];
            nativeBuffer = (unsigned char *)malloc(length);
            memcpy(nativeBuffer, bytes, length);
        }
        
        dispatch_async(dispatch_get_main_queue(), ^{
            
             if (nativeBuffer) {
                const char *cAssetID = [assetID UTF8String];

                // 此时肯定在主线程，调用 C# 安全
                callback(nativeBuffer, (int)length, cAssetID);

                free(nativeBuffer);
            } else {
                const char *cAssetID = [assetID UTF8String];
                NSLog(@"[PhotoLib] 内存分配失败");
                callback(NULL, 0, cAssetID);
            }
            
        }); // dispatch_async 结束
    }];
} 
@end