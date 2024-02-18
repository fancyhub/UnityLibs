/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/19
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// 平台类型, 一个平台可能对应多个 Account登录方式
    /// 比如 一些集成了 Google,FaceBook
    /// 一些同时集成了 WeChat,QQ等登录方式
    /// </summary>
    public enum ELoginPlatform
    {
        None, //这个不要删除

        Device,
        Dev,
        GMsdk,

        Max,//这个不要删除
    }

    /// <summary>
    /// 账号类型
    /// </summary>
    public enum ELoginPlatformAccount
    {
        None,//这个不要删除

        Device_GUID,  // device
        Dev_AccountName, //dev

        //MSDK Begin

        GMsdk_Garena,
        GMsdk_Facebook,
        GMsdk_Guest,
        GMsdk_Vkontakte,
        GMsdk_Line,
        GMsdk_Huawei,
        GMsdk_Google,
        GMsdk_Apple,
        GMsdk_Twitter,
        GMsdk_Email,
        GMsdk_GooglePlayGames,        
        //MSDK End
    }
}
