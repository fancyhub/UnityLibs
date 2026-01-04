/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>
#include <string.h>


static NSString* FH_Global_IDFA = nil;
static bool FH_IDFA_Requested = false; // 标记是否已经请求过授权，避免重复请求

extern "C" {

// 请求跟踪权限并获取 IDFA
void _RequestIDFA()
{
    // 如果已经请求过，就不再重复请求
    if (FH_IDFA_Requested) {
        return;
    }
    FH_IDFA_Requested = true;
    
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            // 直接获取 IDFA 字符串，不检查授权状态，由外部判断是否有效
            NSUUID *idfa = [[ASIdentifierManager sharedManager] advertisingIdentifier];
            FH_Global_IDFA = [idfa UUIDString]; // NSString 是不可变的，线程安全
        }];
    } else {
        // 直接获取 IDFA 字符串，不检查 isAdvertisingTrackingEnabled，由外部判断是否有效
        ASIdentifierManager *manager = [ASIdentifierManager sharedManager];
        NSUUID *idfa = [manager advertisingIdentifier];
        FH_Global_IDFA = [idfa UUIDString]; // NSString 是不可变的，线程安全
    }
}

// 仅获取当前 IDFA（不请求权限，若未授权则返回零值）
// 使用 strdup 分配新内存，Unity IL2CPP 会自动释放
const char* FH_GetIDFA()
{
    if (FH_Global_IDFA != nil) {
       
        return strdup([FH_Global_IDFA UTF8String]);
    }
    
    if (!FH_IDFA_Requested) {
        _RequestIDFA();
    }
    return NULL;
}

bool FH_IsIDFAReady()
{
    if (FH_Global_IDFA != nil)
        return true;
    
    if (!FH_IDFA_Requested) {
        _RequestIDFA();
    }
    return false;
}
}
 
