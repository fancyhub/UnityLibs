/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    [CreateAssetMenu(fileName = "NoticeConfig", menuName = "fanchhub/NoticeConfig")]
    [Serializable]
    public class NoticeConfigAsset : ScriptableObject
    {
        public const string CPath = "Assets/Res/UI/Config/NoticeConfig.asset";

        public ELogLvl LogLvl = ELogLvl.Info;
        public List<NoticeConfig> Channels = new List<NoticeConfig>();
    }

    [System.Serializable]
    public sealed class NoticeDummyConfig
    {
        public string LayerName;

        [Range(0f, 1f)]
        public float PosX = 0.5f;

        [Range(0f, 1f)]
        public float PosY = 0.3f;

        public Vector2 Pos
        {
            get { return new Vector2(PosX, PosY);}
        }
    }

    /// <summary>
    /// 一个频道的所有配置，包括移动和表现
    /// </summary>
    [System.Serializable]
    public class NoticeConfig
    {
        public ENoticeChannel ChannelType;
        public NoticeDummyConfig Dummy = new NoticeDummyConfig();
        public NoticeContainerConfig Container= new NoticeContainerConfig();
        public NoticeChannelConfig Channel= new NoticeChannelConfig();
    }
}
