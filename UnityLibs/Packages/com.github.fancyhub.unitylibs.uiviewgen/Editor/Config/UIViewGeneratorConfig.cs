using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FH.UI.ViewGenerate.Ed
{
    [CreateAssetMenu(menuName = "UIView/UI View Gen Config", fileName = "UIViewGeneratorConfig")]
    public class UIViewGeneratorConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum EPathMode
        {
            AssetPath,
            ResourcePath,
            PrefabName,
        }

        public const string DefaultPath = "Assets/fancyhub/UIViewGeneratorConfig.asset";
        public const string C_EditorPrefs_Key = "fancyhub.uiview.gen.path";

        public const string C_MENU_Gen_Select = "Assets/Gen UIView Code";
        public const string C_MENU_Gen_ALL = "Tools/UI/Regen All UIView Code";
        public const string C_MENU_Clear_Unused_Class = "Tools/UI/Clear UIView Code";
        public const string C_MENU_Export_Class_Usage = "Tools/UI/Gen Class Usage";

        public CSharpConfig Csharp = new CSharpConfig();

        [Serializable]
        public sealed class CSharpConfig
        {
            public const string ExtSuffix = ".ext.cs";
            public const string ResSuffix = ".res.cs";

            public string NameSpace = "FH.UI";
            public string Prefix = "UI"; //自动生成的 class 前缀
            public string Suffix = "View";
            public string BaseClassName = "FH.UI.UIBaseView";
            public string CodeFolder = "Assets/Scripts/UI/View";

            public EPathMode PathMode = EPathMode.AssetPath;
            public string ResourcePath = "/Resources/";

            public Type _BaseViewClass;


            public string GenClassName(string prefab_path)
            {
                return ClassNameUtil.GenClassNameFromPath(prefab_path, Prefix, Suffix);
            }

            public string GenFilePath_Ext(string prefab_path)
            {
                string class_name = GenClassName(prefab_path);
                return System.IO.Path.Combine(CodeFolder, class_name + ExtSuffix);
            }

            public string GenFilePath_Res(string prefab_path)
            {
                string class_name = GenClassName(prefab_path);
                return System.IO.Path.Combine(CodeFolder, class_name + ResSuffix);
            }
        }


        public List<string> ResourcesFolderList = new List<string>()
        {
            "Assets/Res/UI/Prefab"
        };

        public List<string> PriorityCompTypeList = new List<string>()
        {
            "UnityEngine.UI.Button",
            "UnityEngine.UI.Toggle",
            "UnityEngine.UI.Slider",
            "UnityEngine.UI.InputField",
            "UnityEngine.UI.ScrollRect",
            "UnityEngine.UI.Text",
            "UnityEngine.UI.Scrollbar",
            "UnityEngine.UI.RawImage",
            "UnityEngine.UI.Image",
            "UnityEngine.RectTransform",
        };

        public bool IsPrefabPathValid(string path)
        {
            foreach (var key_word in ResourcesFolderList)
            {
                if (path.StartsWith(key_word))
                    return true;
            }

            //Debug.LogError("Prefab 路径不合法 " + path);
            return false;
        }


        public string GetSelfPath()
        {
            return AssetDatabase.GetAssetPath(this);
        }

        public void SetCurrentDefault()
        {
            UnityEditor.EditorPrefs.SetString(C_EditorPrefs_Key, GetSelfPath());
        }
        public bool IsCurrentDefault()
        {

            string path = UnityEditor.EditorPrefs.GetString(C_EditorPrefs_Key);
            if (!string.IsNullOrEmpty(path))
                return path == GetSelfPath();

            return GetSelfPath() == DefaultPath;
        }

        public static UIViewGeneratorConfig LoadDefault()
        {
            UIViewGeneratorConfig ret = null;
            //PlayerPrefs.GetFloat
            string path = UnityEditor.EditorPrefs.GetString(C_EditorPrefs_Key);
            if (!string.IsNullOrEmpty(path))
            {
                ret = AssetDatabase.LoadAssetAtPath<UIViewGeneratorConfig>(path);
                if (ret != null)
                    return ret;

                Debug.LogWarning("当前配置路径无效 " + path);
            }

            ret = AssetDatabase.LoadAssetAtPath<UIViewGeneratorConfig>(DefaultPath);
            Debug.Assert(ret != null, "加载 UIViewGenConfig 失败 " + DefaultPath);
            return ret;
        }


        public List<Type> _PriorityCompTypeList = new List<Type>();

        public Component GetComponent(Transform target, Transform root)
        {
            foreach (System.Type t in _PriorityCompTypeList)
            {
                Component obj = target.GetComponent(t);
                if (null != obj)
                    return obj;
            }

            //如果不是根节点，但是 _开头，就把transform导出
            if (target != root)
            {
                return target.GetComponent<Transform>();
            }

            return null;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if UNITY_EDITOR
            _PriorityCompTypeList.Clear();

            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Csharp._BaseViewClass = assembly.GetType(Csharp.BaseClassName, false, false);
                    if (Csharp._BaseViewClass != null)
                        break;
                }
                if (Csharp._BaseViewClass == null)
                {
                    Debug.LogError("找不View的基类 " + Csharp.BaseClassName);
                }
            }


            UnityEditor.TypeCache.TypeCollection sub_class_list = UnityEditor.TypeCache.GetTypesDerivedFrom<UnityEngine.Component>();
            foreach (var p in PriorityCompTypeList)
            {
                bool found = false;
                foreach (var type in sub_class_list)
                {
                    if (type.FullName == p)
                    {
                        _PriorityCompTypeList.Add(type);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError("找不到类型 " + p);
                }
            }
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
    }



}
