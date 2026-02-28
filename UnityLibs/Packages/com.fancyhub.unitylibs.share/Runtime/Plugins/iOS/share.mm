
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/26
 * Title   :
 * Desc    :
 *************************************************************************************/

#import <Foundation/Foundation.h>
#import <Photos/Photos.h>
#import <UIKit/UIKit.h>
#import <UIKit/UIPopoverSupport.h>
#import <Unity/UnityInterface.h>
#include <stdlib.h>

// 内部辅助类
@interface FHShareUtil : NSObject
+ (const char*)getLatestPhotoAssetIDSynchronous;
+ (void)share:(NSString*)title text:(NSString*)text imagePath:(NSString*)imagePath;
@end

static id _notificationObserver = nil;

extern "C" {

typedef void (*FHScreenshotEventCallback)();
static FHScreenshotEventCallback s_ScreenshotEventCallback = NULL;

void FHStartScreenShotListener(FHScreenshotEventCallback callBack)
{
    s_ScreenshotEventCallback = callBack;

    if (_notificationObserver) {
        return;
    }

    _notificationObserver = [[NSNotificationCenter defaultCenter] addObserverForName:UIApplicationUserDidTakeScreenshotNotification
                                                                              object:nil
                                                                               queue:nil
                                                                          usingBlock:^(NSNotification* _Nonnull note) {
                                                                              // 【关键】如果回调指针有效，则调用它
                                                                              if (s_ScreenshotEventCallback != NULL) {
                                                                                  s_ScreenshotEventCallback();
                                                                              } else {
                                                                                  NSLog(@"[ScreenShotListener] 警告：回调指针为空，无法通知 Unity");
                                                                              }
                                                                          }];
}

void FHStopScreenShotListener()
{
    s_ScreenshotEventCallback = NULL;

    if (_notificationObserver) {
        [[NSNotificationCenter defaultCenter] removeObserver:_notificationObserver];
        _notificationObserver = nil;
        NSLog(@"[ScreenShotListener] 监听已停止");
    }
}

const char* FHGetLatestPhotoAssetID()
{
    return [FHShareUtil getLatestPhotoAssetIDSynchronous];
}

// 保存图片到相册（返回值：0=成功，1=无权限，2=未知错误）
int FHSaveImageToPhotoAlbum(const char* imagePath)
{
    // 1. 空路径直接返回未知错误
    if (imagePath == NULL || strlen(imagePath) == 0) {
        NSLog(@"保存失败：图片路径为空");
        return 2;
    }

    NSString* path = [NSString stringWithUTF8String:imagePath];
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

        if (status == PHAuthorizationStatusNotDetermined){            

            if (@available(iOS 14, *)) {
                [PHPhotoLibrary requestAuthorizationForAccessLevel:PHAccessLevelAddOnly handler:^(PHAuthorizationStatus status) {
                    //Do nothing                    
                }];
            } else {
                [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
                    //Do nothing
                }];
            }             
        }
        return 1;
    }

    // 3. 检查文件是否存在
    if (![[NSFileManager defaultManager] fileExistsAtPath:path]) {
        NSLog(@"保存失败：图片文件不存在 - %@", path);
        return 2;
    }

    // 4. 同步写入相册 (使用 performChangesAndWait 简化逻辑)
    NSError* error = nil;
    BOOL success = [[PHPhotoLibrary sharedPhotoLibrary]
        performChangesAndWait:^{
            // 尝试创建请求
            NSURL* fileURL = [NSURL fileURLWithPath:path];
            // 如果文件不是有效的图片，这里可能不会立即报错，但提交时会失败
            [PHAssetCreationRequest creationRequestForAssetFromImageAtFileURL:fileURL];
        }
                        error:&error];

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
    NSString* ocTitle = title ? [NSString stringWithUTF8String:title] : @"";
    NSString* ocText = text ? [NSString stringWithUTF8String:text] : @"";
    NSString* ocImagePath = imagePath ? [NSString stringWithUTF8String:imagePath] : @"";

    // 调用分享逻辑
    [FHShareUtil share:ocTitle text:ocText imagePath:ocImagePath];
}
}

@implementation FHShareUtil
+ (const char*)getLatestPhotoAssetIDSynchronous
{
    PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
    if (status != PHAuthorizationStatusAuthorized) {
        return NULL;
    }

    PHFetchOptions* options = [[PHFetchOptions alloc] init];
    options.sortDescriptors = @[ [NSSortDescriptor sortDescriptorWithKey:@"creationDate" ascending:NO] ];
    options.fetchLimit = 1;
    options.predicate = [NSPredicate predicateWithFormat:@"mediaType == %d", PHAssetMediaTypeImage];

    PHFetchResult<PHAsset*>* result = [PHAsset fetchAssetsWithOptions:options];

    if (result.count > 0) {
        PHAsset* asset = [result firstObject];
        NSString* assetID = asset.localIdentifier;

        return strdup([assetID UTF8String]);
    }
    return NULL;
}

+ (void)share:(NSString*)title text:(NSString*)text imagePath:(NSString*)imagePath
{    
    // 1. 参数校验
    if ((text == nil || text.length == 0) && (imagePath == nil || imagePath.length == 0)) {
        NSLog(@"UnityShare: 文字和图片都为空，不执行分享");
        return;
    }

    // 2. 组装分享内容
    NSMutableArray* activityItems = [NSMutableArray array];

    // 处理文字
    if (text.length > 0) {
        NSString* shareText = text;
        if (title.length > 0) {
            shareText = [NSString stringWithFormat:@"%@\n%@", title, text];
        }
        [activityItems addObject:shareText];
    }

    // 处理图片
    UIImage* shareImage = nil;
    if (imagePath.length > 0) {
        // 【修正】Unity 传过来的通常是绝对路径 (Application.persistentDataPath/...)
        // 如果 imagePath 已经是绝对路径，直接使用；如果是相对路径，可能需要拼接
        NSString* fullImagePath = imagePath;
        
        // 简单判断：如果不是以 / 开头，可能是相对路径，尝试拼接 Documents (视你的 Unity 代码而定)
        if (![imagePath hasPrefix:@"/"]) {
            NSString* docsDir = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES).firstObject;
            fullImagePath = [docsDir stringByAppendingPathComponent:imagePath];
        }

        if ([[NSFileManager defaultManager] fileExistsAtPath:fullImagePath]) {
            shareImage = [UIImage imageWithContentsOfFile:fullImagePath];
            if (shareImage) {
                [activityItems addObject:shareImage];
            } else {
                NSLog(@"UnityShare: 图片加载失败 (格式错误或损坏): %@", fullImagePath);
            }
        } else {
            NSLog(@"UnityShare: 图片文件不存在: %@", fullImagePath);
        }
    }

    if (activityItems.count == 0) {
        NSLog(@"UnityShare: 最终没有有效的内容可分享");
        return;
    }

    // 3. 创建分享控制器
    UIActivityViewController* activityVC = [[UIActivityViewController alloc]
                                            initWithActivityItems:activityItems
                                            applicationActivities:nil];

    // 4. 适配 iPad (Popover 设置) - 必须配置，否则 iPad 会崩溃
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        activityVC.modalPresentationStyle = UIModalPresentationPopover;
        
        // 【核心修复】获取最顶层的 ViewController，兼容 iOS 13+ SceneDelegate
        UIViewController* topVC = [self getTopViewController];
        
        if (topVC && topVC.view) {
            UIPopoverPresentationController* popover = activityVC.popoverPresentationController;
            popover.sourceView = topVC.view;
            // 设置在屏幕中心弹出
            popover.sourceRect = CGRectMake(CGRectGetMidX(topVC.view.bounds),
                                            CGRectGetMidY(topVC.view.bounds),
                                            0, 0);
            popover.permittedArrowDirections = UIPopoverArrowDirectionAny;
        } else {
            NSLog(@"UnityShare: 警告 - 未能获取到有效的 Top ViewController，iPad 弹窗可能异常");
        }
    }

    // 5. 在主线程弹出
    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController* topVC = [self getTopViewController];
        
        if (!topVC) {
            NSLog(@"UnityShare: 错误 - 无法获取 ViewController 来呈现分享面板");
            return;
        }

        // 检查是否已经有其他 VC 正在呈现，如果有，尝试找到最上层的
        while (topVC.presentedViewController) {
            topVC = topVC.presentedViewController;
        }

        // 再次检查视图是否在窗口层级中 (防止场景切换导致的崩溃)
        if (topVC.view.window) {
            [topVC presentViewController:activityVC animated:YES completion:nil];
        } else {
            // 极端情况：视图不在窗口中，尝试回退到 root
            UIViewController* rootVC = [self getTopViewController]; 
            if(rootVC) {
                 [rootVC presentViewController:activityVC animated:YES completion:nil];
            }
        }
    });
}


// --- 核心工具方法：兼容 iOS 13+ 获取最顶层 ViewController ---
+ (UIViewController*)getTopViewController {    
    extern UIViewController* UnityGetGLViewController();
    return  UnityGetGLViewController();    
}
@end
