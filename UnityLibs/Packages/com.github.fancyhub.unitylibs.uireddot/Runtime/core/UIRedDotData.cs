/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/17 11:29:44
 * Title   : 
 * Desc    : 红点的数据
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public class UIRedDotData
    {
        public static UIRedDotData Inst = new UIRedDotData();

        public UIRedDotTree _Tree;
        public UIRedDotVirual _Virtual;
        private EventSet2<Str, int> _EventSet;
        private List<(Str, int)> _ChangeList;

        public UIRedDotData()
        {
            _Tree = new UIRedDotTree();
            _Virtual = new UIRedDotVirual(_Tree);
            _EventSet = new EventSet2<Str, int>();
            _ChangeList = new List<(Str, int)>();
        }

        public void Clear()
        {
            _Tree.Destroy();
            _EventSet.Clear();            
        }

        public static void AddVirtualLink(string real_path, string virual_path)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(real_path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return;
            }
            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRdData 实例还没有创建");
                return;
            }

            inst._ChangeList.Clear();
            if (!inst._Virtual.Add(real_path, virual_path, inst._ChangeList))
                return;

            UIRedDotLog.D("Begin AddVirtualLink Path: {0} {1}", real_path, virual_path);
            inst._Virtual.Update(inst._ChangeList);
            foreach (var p in inst._ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.Item1);
                inst._EventSet.FireAsync(p.Item1, p.Item2);
            }
            inst._ChangeList.Clear();
            UIRedDotLog.D("End AddVirtualLink Path: {0} {1}", real_path, virual_path);
        }

        public static void ReomveVirtualLink(string real_path, string virual_path)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(real_path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return;
            }

            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRdData 实例还没有创建");
                return;
            }

            inst._ChangeList.Clear();
            if (!inst._Virtual.Remove(real_path, virual_path, inst._ChangeList))
                return;

            UIRedDotLog.D("Begin ReomveVirtualLink Path: {0} {1}", real_path, virual_path);
            inst._Virtual.Update(inst._ChangeList);

            foreach (var p in inst._ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.Item1);
                inst._EventSet.FireAsync(p.Item1, p.Item2);
            }
            inst._ChangeList.Clear();

            UIRedDotLog.D("End ReomveVirtualLink Path: {0} {1}", real_path, virual_path);
        }

        public static bool Reg(string path, EventSet2<Str, int>.IHandler call_back)
        {
            if (string.IsNullOrEmpty(path))
            {
                UIRedDotLog.Assert(false, "路径不能为空");
                return false;
            }
            if (call_back == null)
            {
                UIRedDotLog.Assert(false, "回调不能为空");
                return false;
            }

            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRdData 实例还没有创建");
                return false;
            }

            bool ret = inst._EventSet.Reg(path, call_back);
            if (!ret)
                return false;

            if (inst._Tree.TryGet(path, out var v))
                call_back.HandleEvent(path, v.Value);
            else
                call_back.HandleEvent(path, 0);
            return true;
        }

        public static void UnReg(string path, EventSet2<Str, int>.IHandler call_back)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var inst = Inst;
            if (inst == null)
                return;
            inst._EventSet.Unreg(path, call_back);
        }

        public static void Set(string path, int count = 1)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRdData 实例还没有创建");
                return;
            }
            if (!inst._Tree.Set(path, count, EUIRedDotDataType.ManualNode))
                return;

            UIRedDotLog.D("Begin Set Path: {0} {1}", path, count);
            inst._ChangeList.Clear();
            //如果是手动类型的节点, 并且值为0,就删除
            if (count == 0 &&
                inst._Tree.TryGet(path, out var temp_v) &&
                temp_v.DataType == EUIRedDotDataType.ManualNode)
            {
                inst._Tree.Delete(path, inst._ChangeList);
            }
            else
            {
                inst._ChangeList.Add((path, count));
            }

            inst._Tree.UpdateParent(path, inst._ChangeList);
            inst._Virtual.Update(inst._ChangeList);
            foreach (var p in inst._ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.Item1);
                inst._EventSet.FireAsync(p.Item1, p.Item2);
            }
            inst._ChangeList.Clear();

            UIRedDotLog.D("End Set Path: {0} {1}", path, count);
        }

        public static void Del(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            UIRedDotLog.D("Begin Del Path: {0}", path);
            var inst = Inst;
            if (inst == null)
            {
                UIRedDotLog.Assert(false, "UIRdData 实例还没有创建");
                return;
            }

            inst._ChangeList.Clear();
            if (!inst._Tree.Delete(path, inst._ChangeList))
            {
                //该节点不存在
                return;
            }
            inst._Tree.UpdateParent(path, inst._ChangeList);
            inst._Virtual.Update(inst._ChangeList);
            foreach (var p in inst._ChangeList)
            {
                UIRedDotLog.D("\tPath Event: {0}", p.Item1);
                inst._EventSet.FireAsync(p.Item1, p.Item2);
            }
            inst._ChangeList.Clear();

            UIRedDotLog.D("End Del Path: {0}", path);
        }

        public void Update()
        {
            _EventSet.ProcessAllEvents();
        }
    }
}
