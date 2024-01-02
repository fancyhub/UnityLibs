/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 设置 View的ParentView
    /// </summary>
    public class LinkViewProcessor : IViewGeneratePreprocessor
    {
        public void Process(EdUIViewGenContext context)
        {
            Dictionary<string, EdUIView> dict = new Dictionary<string, EdUIView>(context.ViewList.Count);
            foreach (var p in context.ViewList)
            {
                dict.Add(p.Desc.ClassName, p);
            }

            //把父类link起来
            foreach (EdUIView view in context.ViewList)
            {
                if (!view.IsVariant)
                    continue;

                if(!dict.TryGetValue(view.ParentViewName,out view.ParentView) || view.ParentView==null)
                {
                    Debug.LogError("can't find parent class " + view.ParentViewName);
                }
            }
        }
    }
}