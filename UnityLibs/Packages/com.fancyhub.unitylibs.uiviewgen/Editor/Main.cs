/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace FH.UI.ViewGenerate.Ed
{
    public static class Main
    {
        [MenuItem(UIViewGeneratorConfig.C_MENU_Gen_Select, true, 200)]
        public static bool GenCode_Select_Valid()
        {
            GameObject[] selections = Selection.gameObjects;
            if (null == selections)
                return false;

            UIViewGeneratorConfig config = UIViewGeneratorConfig.LoadDefault();
            if (config == null)
                return false;

            foreach (GameObject obj in selections)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (config.IsPrefabPathValid(path))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 生成代码，只是当前选中的 prefab
        /// </summary>
        [MenuItem(UIViewGeneratorConfig.C_MENU_Gen_Select, false, 200)]
        public static void GenCode_Select()
        {
            UIViewGeneratorConfig config = UIViewGeneratorConfig.LoadDefault();
            if (config == null)
                return;

            GameObject[] selections = Selection.gameObjects;
            if (null == selections)
            {
                Debug.LogError("选中 prefab 才能生成");
                return;
            }

            foreach (GameObject obj in selections)
            { 
                ViewGenerator view_generator = new ViewGenerator(config);
                view_generator.GenCode(obj);
            }

            //这里之前不加refresh可能是因为生成代码对prefab没有影响
            //但是如果现在不加的话，可能会导致检测不到prefab已经加了脚本
            //但是由于有刷新操作的话，可能会导致GenAll时间比较长，差不多要半小时
            AssetDatabase.Refresh();
            Debug.Log("DONE");
            EditorUtility.DisplayDialog("完成", "结束", "确认");
        }

        [MenuItem(UIViewGeneratorConfig.C_MENU_Gen_ALL, false, 100)]
        public static void ReGenCodeAll()
        {
            UIViewGeneratorConfig config = UIViewGeneratorConfig.LoadDefault();
            if (config == null)
                return;

            ViewGenerator view_generator = new ViewGenerator(config);
            view_generator.RegenAllCode();


            AssetDatabase.Refresh();
            Debug.Log("DONE");
            EditorUtility.DisplayDialog("完成", "结束", "确认");
        }

        /// <summary>
        /// 删除不要的代码
        /// </summary>
        [MenuItem(UIViewGeneratorConfig.C_MENU_Clear_Unused_Class, false, 150)]
        public static void RemoveInvalidateCS()
        {
            UIViewGeneratorConfig config = UIViewGeneratorConfig.LoadDefault();
            if (config == null)
                return;

            ViewGenerator view_generator = new ViewGenerator(config);
            view_generator.RemoveInvalidateCodeFiles();
            Debug.Log("DONE");
        }

        [MenuItem(UIViewGeneratorConfig.C_MENU_Export_Class_Usage, false, 200)]
        public static void GenClassUsage()
        {
            UIViewGeneratorConfig config = UIViewGeneratorConfig.LoadDefault();
            if (config == null || config.Csharp._BaseViewClass == null)
                return;

            string file_path = UnityEditor.EditorUtility.SaveFilePanel("保存", null, null, "csv");
            if (string.IsNullOrEmpty(file_path))
                return;

            var all_types = EdUIViewUsageFinder.Find(config.Csharp._BaseViewClass);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(file_path);
            sw.WriteLine("Type,Owner");
            foreach (var p in all_types)
            {
                sw.Write(p.Key.Name);
                sw.Write(",");
                if (p.Value == null)
                    sw.Write("null");
                else
                    sw.Write(p.Value.Name);
                sw.WriteLine();
            }
            sw.Close();
            UnityEngine.Debug.Log("Done");
        }      
    }
}
