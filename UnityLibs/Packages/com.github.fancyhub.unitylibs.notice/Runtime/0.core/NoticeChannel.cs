/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;


namespace FH
{
    //一个container 对应一个 channel
    public interface INoticeChannel
    {
        //当前是否可以显示
        public bool IsVisible();

        public void Update();
        public void Destroy();

        public void SetVisibleFlag(ENoticeVisibleFlag flag);
        public void Clear();

        public void Push(NoticeData data);
    }
}
