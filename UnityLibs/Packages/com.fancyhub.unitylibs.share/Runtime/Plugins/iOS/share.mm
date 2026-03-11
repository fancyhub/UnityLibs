
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


extern UIViewController* UnityGetGLViewController(); 
  

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
    //1. 处理文本	
	 NSString* shareText = nil;
	 if (text!=nil && text.length > 0) {
        shareText = text;
        if (title!=nil && title.length > 0) {
            shareText = [NSString stringWithFormat:@"%@\n%@", title, text];
        }        
    }
	 
	//2. 处理图片
    UIImage *shareImage=nil;
	if( imagePath!=nil && imagePath.length > 0 )
	{
         NSString* fullImagePath = imagePath;
         if (![imagePath hasPrefix:@"/"]) {
            NSString* docsDir = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES).firstObject;
            fullImagePath = [docsDir stringByAppendingPathComponent:imagePath];
         }

         if ([[NSFileManager defaultManager] fileExistsAtPath:fullImagePath]) {
            shareImage = [UIImage imageWithContentsOfFile:fullImagePath];            
         }	
	}

    //3. 处理分享内容
    NSMutableArray *items = [NSMutableArray new];
    if(shareText!=nil && shareImage==nil)//如果图片为空，则添加文本, 因为有些app,比如fb不支持两个一起
    {
        [items addObject:shareText];        
    }
    if(shareImage!=nil)    
    {
        [items addObject:shareImage];
    }

	if( [items count] == 0 )
	{
		NSLog( @"Share canceled because there is nothing to share..." );
		return;
	}
	
    //4. 获取Top View Controller
    UIViewController *rootViewController = UnityGetGLViewController();
    if(rootViewController == nil)
    {
        NSLog( @"Share canceled: rootViewController is nil");
        return;
    }

    //5. 创建分享视图控制器
	UIActivityViewController *activity = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil];	    

    //5.1 ipad 设置分享视图控制器样式
    if( [[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad ) // iPad
	{
		activity.modalPresentationStyle = UIModalPresentationPopover;
        UIPopoverPresentationController* popover = activity.popoverPresentationController;
		popover.sourceRect = CGRectMake( 
            CGRectGetMidX(rootViewController.view.bounds), 
            CGRectGetMidX(rootViewController.view.bounds), 
            0, 0 );
		popover.sourceView = rootViewController.view;
		popover.permittedArrowDirections = UIPopoverArrowDirectionAny;        
	}	 

    //5.2 设置分享完成回调
	activity.completionWithItemsHandler = ^( UIActivityType activityType, BOOL completed, NSArray *returnedItems, NSError *activityError )
	{
		if( activityError != nil )
			NSLog( @"Share error: %@", activityError );		
	};

    //5.3 设置分享排除类型
	activity.excludedActivityTypes = @[
        @"com.apple.UIKit.activity.AirDrop",
        @"com.apple.UIKit.activity.Print",  
        @"com.apple.UIKit.activity.AssignToContact"
    ];

	
    //6. 显示分享视图控制器
    [rootViewController presentViewController:activity animated:YES completion:nil];
}
 
@end
