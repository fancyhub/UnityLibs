/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/
#import <Foundation/Foundation.h>
#import <Security/Security.h>

extern "C" {

// 返回设备可用磁盘空间（字节）
long long FH_GetFreeDiskSpace()
{
    NSError *error = nil;
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths firstObject];
    NSDictionary *fileAttributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath:documentsDirectory error:&error];
    if (error) {
        return -1;
    }
    NSNumber *freeSpace = [fileAttributes objectForKey:NSFileSystemFreeSize];
    return [freeSpace longLongValue];
}

// 返回设备总磁盘空间（字节）
long long FH_GetTotalDiskSpace()
{
    NSError *error = nil;
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths firstObject];
    NSDictionary *fileAttributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath:documentsDirectory error:&error];
    if (error) {
        return -1;
    }
    NSNumber *totalSpace = [fileAttributes objectForKey:NSFileSystemSize];
    return [totalSpace longLongValue];
}
}
 


static NSDictionary *FH_ParseMobileProvision()
{
    NSString *path = [[NSBundle mainBundle] pathForResource:@"embedded"
                                                     ofType:@"mobileprovision"];
    if (!path) return nil;

    NSData *data = [NSData dataWithContentsOfFile:path];
    if (!data) return nil;

    NSString *content = [[NSString alloc] initWithData:data
                                              encoding:NSISOLatin1StringEncoding];
    if (!content) return nil;

    NSRange start = [content rangeOfString:@"<plist"];
    NSRange end = [content rangeOfString:@"</plist>"];

    if (start.location == NSNotFound || end.location == NSNotFound) {
        return nil;
    }

    NSUInteger plistLength = end.location + end.length - start.location;
    NSString *plistString = [content substringWithRange:NSMakeRange(start.location, plistLength)];
    NSData *plistData = [plistString dataUsingEncoding:NSUTF8StringEncoding];

    if (!plistData) return nil;

    id plist = [NSPropertyListSerialization propertyListWithData:plistData
                                                         options:NSPropertyListImmutable
                                                          format:nil
                                                           error:nil];

    if (![plist isKindOfClass:[NSDictionary class]]) {
        return nil;
    }

    return (NSDictionary *)plist;
}


/*
0 = Unknown
1 = Development
2 = AdHoc
3 = Enterprise
4 = TestFlight
5 = AppStore
*/
extern "C" int FH_GetIOSDistributionType()
{
    NSDictionary *profile = FH_ParseMobileProvision();

    if (profile) {
        NSDictionary *entitlements = profile[@"Entitlements"];

        BOOL getTaskAllow = [entitlements[@"get-task-allow"] boolValue];
        BOOL provisionsAllDevices = [profile[@"ProvisionsAllDevices"] boolValue];
        id provisionedDevices = profile[@"ProvisionedDevices"];

        if (getTaskAllow) {
            return 1; // Development
        }

        if (provisionsAllDevices) {
            return 3; // Enterprise
        }

        if ([provisionedDevices isKindOfClass:[NSArray class]]) {
            return 2; // AdHoc
        }

        return 0; // Unknown
    }

    NSURL *receiptURL = [[NSBundle mainBundle] appStoreReceiptURL];
    NSString *receiptPath = receiptURL.path;

    if (receiptPath && [receiptPath containsString:@"sandboxReceipt"]) {
        return 4; // TestFlight
    }

    if (receiptPath && [[NSFileManager defaultManager] fileExistsAtPath:receiptPath]) {
        return 5; // AppStore
    }

    return 0; // Unknown
}

extern "C" int FH_HasIOSEntitlement(const char *entitlementKey)
{
    if (!entitlementKey || entitlementKey[0] == '\0') {
        return 0;
    }

    NSString *key = [NSString stringWithUTF8String:entitlementKey];
    if (!key || key.length == 0) {
        return 0;
    }

    SecTaskRef task = SecTaskCreateFromSelf(NULL);
    if (!task) {
        return 0;
    }

    CFTypeRef value = SecTaskCopyValueForEntitlement(task, (__bridge CFStringRef)key, NULL);
    CFRelease(task);

    if (!value) {
        return 0;
    }

    int result = 1;
    CFTypeID typeId = CFGetTypeID(value);

    if (typeId == CFBooleanGetTypeID()) {
        result = CFBooleanGetValue((CFBooleanRef)value) ? 1 : 0;
    } else if (typeId == CFArrayGetTypeID()) {
        result = CFArrayGetCount((CFArrayRef)value) > 0 ? 1 : 0;
    } else if (typeId == CFStringGetTypeID()) {
        result = CFStringGetLength((CFStringRef)value) > 0 ? 1 : 0;
    } else if (typeId == CFDictionaryGetTypeID()) {
        result = CFDictionaryGetCount((CFDictionaryRef)value) > 0 ? 1 : 0;
    } else if (typeId == CFNumberGetTypeID()) {
        result = 1;
    } else {
        result = 1;
    }

    CFRelease(value);
    return result;
}

extern "C" const char *FH_GetIOSEntitlementJson(const char *entitlementKey)
{
    if (!entitlementKey || entitlementKey[0] == '\0') {
        return NULL;
    }

    NSString *key = [NSString stringWithUTF8String:entitlementKey];
    if (!key || key.length == 0) {
        return NULL;
    }

    SecTaskRef task = SecTaskCreateFromSelf(NULL);
    if (!task) {
        return NULL;
    }

    CFTypeRef value = SecTaskCopyValueForEntitlement(task, (__bridge CFStringRef)key, NULL);
    CFRelease(task);

    if (!value) {
        return NULL;
    }

    CFTypeID typeId = CFGetTypeID(value);
    NSString *type = @"unknown";
    id entitlementValue = nil;

    if (typeId == CFBooleanGetTypeID()) {
        type = @"bool";
        entitlementValue = CFBooleanGetValue((CFBooleanRef)value) ? @YES : @NO;
    } else if (typeId == CFStringGetTypeID()) {
        type = @"string";
        entitlementValue = (__bridge NSString *)value;
    } else if (typeId == CFArrayGetTypeID()) {
        type = @"array";
        entitlementValue = (__bridge NSArray *)value;
    } else if (typeId == CFDictionaryGetTypeID()) {
        type = @"dictionary";
        entitlementValue = (__bridge NSDictionary *)value;
    } else if (typeId == CFNumberGetTypeID()) {
        type = @"number";
        entitlementValue = (__bridge NSNumber *)value;
    } else if (typeId == CFDataGetTypeID()) {
        type = @"data";
        entitlementValue = [(__bridge NSData *)value base64EncodedStringWithOptions:0];
    } else {
        entitlementValue = [(__bridge id)value description];
    }

    NSDictionary *object = @{
        @"type": type,
        @"value": entitlementValue ?: [NSNull null]
    };

    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object
                                                       options:0
                                                         error:&error];
    CFRelease(value);

    if (error || !jsonData) {
        return NULL;
    }

    NSString *json = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    if (!json) {
        return NULL;
    }

    return strdup([json UTF8String]);
}

extern "C" void FH_FreeString(const char *str)
{
    if (str) {
        free((void *)str);
    }
}
