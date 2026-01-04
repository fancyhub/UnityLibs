/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/
#import <Foundation/Foundation.h>

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
 