/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/12
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FH.UI
{
    //只有一种
    public class ScrollListItemSingleFactory<TView, TData> : IUIScrollListItemFactory<TData>
        where TView : UIBaseView, IVoSetter<TData>, new()
    {
        public IScrollListItem<TData> CreateScrollItem(TData data)
        {
            return new ScrollListItem<TView, TData>();
        }

        public bool IsScrollItemSuite(IScrollListItem<TData> item, TData data)
        {
            //永远相同
            return true;
        }

        public void CheckViewClickable()
        {
            Log.Assert(typeof(IUISelectable).IsAssignableFrom(typeof(TView)), "{0} 需要实现 IUISelectable 接口, 要不然 UIScrollList:SetItemClickCB 无效", typeof(TView));
        }
    }
}
