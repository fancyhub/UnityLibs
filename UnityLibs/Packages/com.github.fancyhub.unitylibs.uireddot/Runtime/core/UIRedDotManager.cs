/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/17 11:29:44
 * Title   : 
 * Desc    : 红点的数据
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.UI
{
    /// <summary>
    /// 红点系统的数据类
    /// 虚拟节点本身不能被监听
    /// </summary>
    public class UIRedDotMgr
    {
        private static UIRedDotMgr _;
        public static UIRedDotMgr Inst => _;

        private UIRedDotTree _Tree;
        private UIRedDotVirual _Virtual;
        private EventSet2<Str, int> _EventSet;
        private List<(Str path, int value)> _ChangeList;

        private UIRedDotMgr()
        {
            _Tree = new UIRedDotTree();
            _Virtual = new UIRedDotVirual(_Tree);
            _EventSet = new EventSet2<Str, int>();
            _ChangeList = new List<(Str, int)>();
        }

        public static void Init(ELogLvl log_lvl = ELogLvl.Info)
        {
            UIRedDotLog.SetMasks(log_lvl);
            if (_ == null)
                _ = new UIRedDotMgr();
        }

        public static ValueTree<UIRedDotNodeData> RootNode
        {
            get
            {
                if (Inst == null)
                    return null;
                return Inst._Tree._Root;
            }
        }
        public static UIRedDotTree Tree
        {
            get
            {
                if (Inst == null)
                    return null;
                return Inst._Tree;
            }
        }

        public static void Clear()
        {
            if (Inst == null)
                return;

            Inst._Tree.Destroy();
            Inst._EventSet.Clear();
        }

        public static void Link(string real_path, string virtual_path)
        {
            if (_ == null)
            {
                UIRedDotLog.Assert(false, "UIRedDotMgr 实例还没有创建");
                return;
            }

            UIRedDotLog.D("====Begin Link Path: {0} -> {1}", real_path, virtual_path);
            _._Link(real_path, virtual_path);
            UIRedDotLog.D("====End Link Path: {0} -> {1}", real_path, virtual_path);
        }

        public static void Unlink(string real_path, string virtual_path)
        {
            if (_ == null)
            {
                UIRedDotLog.Assert(false, "UIRedDotMgr 实例还没有创建");
                return;
            }

            UIRedDotLog.D("====Begin Unlink Path: {0} {1}", real_path, virtual_path);
            _._Unlink(real_path, virtual_path);
            UIRedDotLog.D("====End Unlink Path: {0} {1}", real_path, virtual_path);
        }


        public static EventSet2<Str, int>.EventHandler Reg(string path, EventSet2<Str, int>.IHandler call_back)
        {
            if (string.IsNullOrEmpty(path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return default;
            }
            if (call_back == null)
            {
                UIRedDotLog.Assert(false, "回调不能为空");
                return default;
            }

            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRedDotMgr 实例还没有创建");
                return default;
            }

            return inst._EventSet.Reg(path, call_back);            
        }

        public static int Get(string path)
        {
            if (Inst == null)
                return 0;

            if (Inst._Tree.TryGet(path, out var v))
                return v.Value;

            return 0;
        }

        public static void Set(string path, int count = 1)
        {
            if (_ == null)
            {
                UIRedDotLog.Assert(false, "UIRedDotMgr 实例还没有创建");
                return;
            }

            UIRedDotLog.D("====Begin Set Path: {0} {1}", path, count);
            _._Set(path, count);
            UIRedDotLog.D("====End Set Path: {0} {1}", path, count);
        }

        public static void Del(string path)
        {
            if (_ == null)
            {
                UIRedDotLog.Assert(false, "UIRedDotMgr 实例还没有创建");
                return;
            }

            UIRedDotLog.D("====Begin Del Path: {0}", path);
            _._Del(path);
            UIRedDotLog.D("====End Del Path: {0}", path);
        }

        public static void Update()
        {
            if (Inst == null)
                return;
            Inst._EventSet.ProcessAllEvents();
        }


        private void _Del(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;


            _ChangeList.Clear();
            if (!_Tree.Delete(path, _ChangeList))
            {
                //该节点不存在
                return;
            }
            _Tree.UpdateParent(path, _ChangeList);
            _Virtual.Update(_ChangeList);
            foreach (var p in _ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.path);
                _EventSet.FireAsync(p.path, p.value);
            }
            _ChangeList.Clear();
        }

        private void _Set(string path, int count = 1)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!_Tree.Set(path, count, EUIRedDotNodeType.ManualNode))
                return;

            _ChangeList.Clear();
            //如果是手动类型的节点, 并且值为0,就删除
            if (count == 0 &&
                _Tree.TryGet(path, out var temp_v) &&
                temp_v.NodeType == EUIRedDotNodeType.ManualNode)
            {
                _Tree.Delete(path, _ChangeList);
            }
            else
            {
                _ChangeList.Add((path, count));
            }

            _Tree.UpdateParent(path, _ChangeList);
            _Virtual.Update(_ChangeList);
            foreach (var p in _ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.path);
                _EventSet.FireAsync(p.path, p.value);
            }
            _ChangeList.Clear();
        }

        public void _Link(string real_path, string virtual_path)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(real_path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return;
            }

            _ChangeList.Clear();
            if (!_Virtual.Link(real_path, virtual_path, _ChangeList))
            {
                UIRedDotLog.Assert(false, "创建虚拟路径失败{0}->{1}", virtual_path, real_path);
                return;
            }

            _Virtual.Update(_ChangeList);
            foreach (var p in _ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.path);
                _EventSet.FireAsync(p.path, p.value);
            }
            _ChangeList.Clear();
        }

        public void _Unlink(string real_path, string virual_path)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(real_path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return;
            }

            _ChangeList.Clear();
            if (!_Virtual.Unlink(real_path, virual_path, _ChangeList))
                return;

            _Virtual.Update(_ChangeList);

            foreach (var p in _ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.path);
                _EventSet.FireAsync(p.path, p.value);
            }
            _ChangeList.Clear();
        }
    }
}
