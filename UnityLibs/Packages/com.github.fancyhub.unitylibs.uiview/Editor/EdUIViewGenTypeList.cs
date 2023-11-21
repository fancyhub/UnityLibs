
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.View.Gen.ED
{
    //这个类需要最外部继承
    public abstract class EdUIViewGenTypeList
    {
        public abstract List<System.Type> GetCompOrderList();

        private static List<System.Type> S_SimpleCompTypeOrderList = new()
        {
            typeof(UnityEngine.RectTransform),
            typeof(UnityEngine.Transform),
        };

        public static List<System.Type> StaticGetCompOrderList()
        {
            List<Type> ret = null;

            //1. 找到子类
            UnityEditor.TypeCache.TypeCollection sub_class_list = UnityEditor.TypeCache.GetTypesDerivedFrom<EdUIViewGenTypeList>();

            //2. 创建子类
            for (int i = 0; i < sub_class_list.Count; i++)
            {
                Type sub_type = sub_class_list[i];
                var type_list = Activator.CreateInstance(sub_type) as EdUIViewGenTypeList;
                if (type_list == null)
                    continue;

                ret = type_list.GetCompOrderList();
                if (ret != null)
                    break;
            }


            //3. 返回
            if (ret == null)
                return S_SimpleCompTypeOrderList;
            if (_NeedMerge(ret))
                ret.AddRange(S_SimpleCompTypeOrderList);
            return ret;
        }

        private static bool _NeedMerge(List<Type> list)
        {
            if (list.Count < S_SimpleCompTypeOrderList.Count)
                return true;

            int start_index = list.Count - S_SimpleCompTypeOrderList.Count;
            for (int i = 0; i < S_SimpleCompTypeOrderList.Count; i++)
            {
                if (list[i + start_index] != S_SimpleCompTypeOrderList[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
