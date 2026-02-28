
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/26
 * Title   :
 * Desc    :
*************************************************************************************/

#import <UIKit/UIKit.h>
#import <UIKit/UIPopoverSupport.h>
#import <Photos/Photos.h>
#import <Foundation/Foundation.h>
#import <Unity/UnityInterface.h>
#include <stdlib.h>

typedef void (*FHPhotoDataCallback)(const unsigned char* dataPtr, int dataLength, const char* assetID);

// 内部辅助类
@interface FHPhotoLibraryHelper : NSObject
+ (const char*)getLatestPhotoAssetIDSynchronous;
+ (void)loadImageDataForAssetID:(NSString *)assetID callback:(FHPhotoDataCallback)callback;
@end


@interface UnityShareManager : NSObject
+ (void)share:(NSString *)title text:(NSString *)text imagePath:(NSString *)imagePath;
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
    [FHPhotoLibraryHelper loadImageDataForAssetID:assetID callback:callBack];
}

// 保存图片到相册（返回值：0=成功，1=无权限，2=未知错误）
int FHSaveImageToPhotoAlbum(const char* imagePath)
{
    // 1. 空路径直接返回未知错误
    if (imagePath == NULL || strlen(imagePath) == 0) {
        NSLog(@"保存失败：图片路径为空");
        return 2;
    }
    
    NSString *path = [NSString stringWithUTF8String:imagePath];
    if (!path) {
        NSLog(@"保存失败：图片路径编码转换失败");
        return 2;
    }
    
    // 2. 检查相册权限
    PHAuthorizationStatus status;
    if (@available(iOS 14, *)) {
        status = [PHPhotoLibrary authorizationStatusForAccessLevel:PHAccessLevelAddOnly];
    } else {
        status = [PHPhotoLibrary authorizationStatus];
    }
    
    BOOL hasPermission = NO;
    if (status == PHAuthorizationStatusAuthorized) {
        hasPermission = YES;
    } else if (@available(iOS 14, *)) {
        if (status == PHAuthorizationStatusLimited) {
            hasPermission = YES;
        }
    }
    
    if (!hasPermission) {
        NSLog(@"保存失败：无相册写入权限，当前状态：%ld", (long)status);
        return 1;
    }
    
    // 3. 检查文件是否存在
    if (![[NSFileManager defaultManager] fileExistsAtPath:path]) {
        NSLog(@"保存失败：图片文件不存在 - %@", path);
        return 2;
    }
    
    // 4. 同步写入相册 (使用 performChangesAndWait 简化逻辑)
    NSError *error = nil;
    BOOL success = [[PHPhotoLibrary sharedPhotoLibrary] performChangesAndWait:^{
        // 尝试创建请求
        NSURL *fileURL = [NSURL fileURLWithPath:path];
        // 如果文件不是有效的图片，这里可能不会立即报错，但提交时会失败
        [PHAssetCreationRequest creationRequestForAssetFromImageAtFileURL:fileURL];
    } error:&error];
    
    if (!success) {
        NSLog(@"保存失败：写入相册出错 - %@", error.localizedDescription);
        // 可以根据 error code 区分更多错误类型，这里统一返回 2
        return 2;
    }
    
    NSLog(@"图片保存成功：%@", path);
    return 0; // 成功
}

/// Unity调用的分享函数
/// @param title 标题（C字符串）
/// @param text 文本（C字符串）
/// @param imagePath 图片路径（C字符串）
void FHShare(const char* title, const char* text, const char* imagePath)
{
    // 转换C字符串为OC字符串
    NSString *ocTitle = title ? [NSString stringWithUTF8String:title] : @"";
    NSString *ocText = text ? [NSString stringWithUTF8String:text] : @"";
    NSString *ocImagePath = imagePath ? [NSString stringWithUTF8String:imagePath] : @"";
    
    // 调用分享逻辑
    [UnityShareManager  share:ocTitle text:ocText imagePath:ocImagePath];
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
    
}
@end


@implementation UnityShareManager
+ (void)shareWithTitle:(NSString *)title text:(NSString *)text imagePath:(NSString *)imagePath {
    // 1. 参数校验：都为空则直接返回
    if ((text == nil || text.length == 0) && (imagePath == nil || imagePath.length == 0)) {
        NSLog(@"UnityShare: 文字和图片都为空，不执行分享");
        return;
    }
    
    // 2. 组装分享内容数组
    NSMutableArray *activityItems = [NSMutableArray array];
    
    // 添加文字（优先加text，title作为补充）
    NSString *shareText = @"";
    if (text.length > 0) {
        shareText = text;
        // 如果有title，拼接到文字开头
        if (title.length > 0) {
            shareText = [NSString stringWithFormat:@"%@\n%@", title, text];
        }
        [activityItems addObject:shareText];
    }
    
    // 添加图片（从Unity沙盒读取图片）
    UIImage *shareImage = nil;
    if (imagePath.length > 0) {
        // Unity的图片路径需要转换为iOS本地路径
        NSString *fullImagePath = [self convertUnityPathToIosPath:imagePath];
        NSData *imageData = [NSData dataWithContentsOfFile:fullImagePath];
        if (imageData) {
            shareImage = [UIImage imageWithData:imageData];
            if (shareImage) {
                [activityItems addObject:shareImage];
            } else {
                NSLog(@"UnityShare: 图片路径无效，无法加载图片: %@", fullImagePath);
            }
        } else {
            NSLog(@"UnityShare: 图片文件不存在: %@", fullImagePath);
        }
    }
    
    // 3. 创建系统分享面板
    UIActivityViewController *activityVC = [[UIActivityViewController alloc]
                                          initWithActivityItems:activityItems
                                          applicationActivities:nil];
    
    // 4. 适配iPad（必做）
    
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        UIViewController *rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
        activityVC.popoverPresentationController.sourceView = rootVC.view;
        activityVC.popoverPresentationController.sourceRect = CGRectMake(CGRectGetMidX(rootVC.view.bounds),
                                                                        CGRectGetMidY(rootVC.view.bounds),
                                                                        0, 0);
        //activityVC.popoverPresentationController.permittedArrowDirections = UIPopoverArrowDirectionNone;
    }
    
    
    // 5. 切换到主线程弹出分享面板（Unity调用可能在子线程）
    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
        [rootVC presentViewController:activityVC animated:YES completion:nil];
    });
}

/// 转换Unity路径为iOS本地路径
+ (NSString *)convertUnityPathToIosPath:(NSString *)unityPath {
    // Unity的Application.persistentDataPath对应iOS的Documents目录
    NSString *documentsPath = [NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES) firstObject];
    
    // 如果Unity传的是相对路径，拼接Documents路径
    if (![unityPath hasPrefix:@"/"]) {
        return [documentsPath stringByAppendingPathComponent:unityPath];
    }
    return unityPath;
}

@end
