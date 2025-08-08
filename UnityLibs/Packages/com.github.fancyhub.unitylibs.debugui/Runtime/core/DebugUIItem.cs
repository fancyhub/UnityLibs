/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH.DebugUI
{
    internal sealed class DebugUIItem
    {
        private static int _IDGen = 0;
        public readonly int Id = ++_IDGen;
        public readonly string Name;

        private List<IDebugUIItemView> _Views;
        private DebugUIItem _Parent;
        private List<DebugUIItem> _Children;

        public DebugUIItem(string name)
        {
            Name = name;
        }

        public List<DebugUIItem> Children => _Children;
        public List<IDebugUIItemView> Views => _Views;

        public bool Add(string path, IDebugUIItemView view)
        {
            if (view == null)
                return false;
            if (string.IsNullOrEmpty(path))
                return false;

            //找到对应的节点
            DebugUIItem view_node = null;
            {
                DebugUIItem cur_node = this;
                string[] temps = path.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < temps.Length - 1; i++)
                {
                    string name = temps[i];
                    cur_node = cur_node._FindOrCreateChildNodeByName(name);
                    if (cur_node == null)
                        return false;
                }

                var last_name = temps[temps.Length - 1];
                view_node = cur_node._FindChildByName(last_name);
                if (view_node == null)
                {
                    view_node = new DebugUIItem(last_name);
                    if (cur_node._Children == null)
                        cur_node._Children = new List<DebugUIItem>();
                    cur_node._Children.Add(view_node);
                    view_node._Parent = cur_node;
                }
            }


            if (view_node == null)
                return false;
            if (view_node._Children != null) //已经是路径节点
                return false;

            if (view_node._Views == null)
                view_node._Views = new List<IDebugUIItemView>();
            view_node._Views.Add(view);

            return true;
        }

        public void Sort()
        {
            if (_Children == null)
                return;
            _Children.Sort((a,b)=>
            {
                return a.Name.CompareTo(b.Name);
            });

            foreach (var p in _Children)
                p.Sort();
        }       


        //这个不能返回里面有View的节点
        private DebugUIItem _FindOrCreateChildNodeByName(string name)
        {
            //自己是显示节点, 不能创建子节点
            if (_Views != null)
                return null;

            var ret = _FindChildByName(name);
            if (ret != null)
            {
                if (ret._Views != null) //这个是view节点, 不能添加路径
                    return null;
                return ret;
            }


            //创建新的
            ret = new DebugUIItem(name);
            if (_Children == null)
                _Children = new List<DebugUIItem>();
            _Children.Add(ret);
            ret._Parent = this;
            return ret;
        }

        private DebugUIItem _FindChildByName(string name)
        {
            if (_Children == null)
                return null;
            foreach (var p in _Children)
            {
                if (p.Name == name)
                    return p;
            }
            return null;
        }
    }

}
