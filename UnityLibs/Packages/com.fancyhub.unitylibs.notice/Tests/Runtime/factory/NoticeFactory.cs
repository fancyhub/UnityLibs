/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace FH.NoticeSample
{
    /// <summary>
    /// container的工厂, 自己实现
    /// </summary>
    internal static class NoticeFactory
    {
        public static INoticeChannel CreateChannel(NoticeConfig config, IClock clock, IResHolder holder)
        {
            INoticeChannelRoot root = _CreateChannelRoot(config, holder);
            if (root == null)
                return null;

            INoticeContainer container = _CreateContainer(config);

            return new NoticeChannel(config.Channel, clock, root, container);
        }

        private static INoticeChannelRoot _CreateChannelRoot(NoticeConfig config, IResHolder holder)
        {
            if (config.ChannelType == ENoticeChannel.None)
                return null;

            //1. 创建Root
            string name = string.Format("ch_{0}_{1}", config.ChannelType, config.Container.ContainerType);

            return new NoticeChannelRoot(config.Dummy, name, holder);
        }

        private static INoticeContainer _CreateContainer(NoticeConfig config)
        {
            switch (config.Container.ContainerType)
            {
                case ENoticeContainer.Single:
                    return new NoticeContainer_Single(config.Container);

                case ENoticeContainer.Multi:
                    return new NoticeContainer_Multi(config.Container);

                //case ENoticeContainer.muti_item_immediate:
                //    return new NoticeContainer_MultiImmediate(config);

                default:
                    NoticeLog.Assert(false, "fault type, please check {0}", config.Container.ContainerType);
                    return null;
            }
        }

        public static RectTransform CreateView(CPtr<IResHolder> resHolder, string path, Transform parent)
        {
            var holder = resHolder.Val;
            if (holder == null)
                return null;

            GameObject obj = holder.Create(path);
            if (obj == null)
                return null;
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }

        public static void ReleaseView(CPtr<IResHolder> resHolder, ref RectTransform view)
        {
            if (view == null)
                return;

            var temp = view.gameObject;
            view = null;

            var holder = resHolder.Val;
            if (holder == null)
            {
                GameObject.Destroy(temp);
            }
            else
            {
                holder.Release(temp);
            }
        }
    }
}
