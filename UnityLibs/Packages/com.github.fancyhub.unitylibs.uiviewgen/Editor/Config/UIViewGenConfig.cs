using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI
{
    [CreateAssetMenu(menuName = "UIView/UI View Gen Config", fileName = "NewViewGeneratorConfig")]
    public class UIViewGenConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string DefaultPath = "Assets/fancyhub/UIGeneratorConfig.asset";
        public const string C_EditorPrefs_Key = "fancyhub.uiview.gen.path";

        public const string C_MENU_Gen_Select = "Assets/Gen UIView Code";
        public const string C_MENU_Gen_ALL = "Tools/UI/Regen All UIView Code";
        public const string C_MENU_Clear_Unused_Class = "Tools/UI/Clear UIView Code";
        public const string C_MENU_Export_Class_Usage = "Tools/UI/Gen Class Usage";

        public string NameSpace = "FH.UI.Sample";
        public string ClassPrefix = "UI"; //自动生成的 class 前缀        
        public string ClassSuffix = "View";
        public string BaseClassName = "FH.UI.Sample.UIBaseView";
        public string CodeFolder = "Assets/Scripts/UI/View";

        public List<string> ResourcesFolderList = new List<string>()
        {
            "Assets/Resources/UI/Prefab"
        };

        public List<string> CompTypeList = new List<string>()
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


        public string EdGetSelfPath()
        {
            return AssetDatabase.GetAssetPath(this);
        }

        public void EdSetCurrentDefault()
        {
            UnityEditor.EditorPrefs.SetString(C_EditorPrefs_Key, EdGetSelfPath());
        }
        public bool EdIsCurrentDefault()
        {
            string path = UnityEditor.EditorPrefs.GetString(C_EditorPrefs_Key);
            if (!string.IsNullOrEmpty(path))
                return path == EdGetSelfPath();

            return EdGetSelfPath() == DefaultPath;
        }

        public static UIViewGenConfig EdLoadDefault()
        {
            UIViewGenConfig ret = null;
            //PlayerPrefs.GetFloat
            string path = UnityEditor.EditorPrefs.GetString(C_EditorPrefs_Key);
            if (!string.IsNullOrEmpty(path))
            {
                ret = AssetDatabase.LoadAssetAtPath<UIViewGenConfig>(path);
                if (ret != null)
                    return ret;

                Debug.LogWarning("当前配置路径无效 " + path);
            }

            ret = AssetDatabase.LoadAssetAtPath<UIViewGenConfig>(DefaultPath);
            Debug.Assert(ret != null, "加载 UIViewGenConfig 失败 " + DefaultPath);
            return ret;
        }

        public Type _BaseViewClass;
        public List<Type> _CompTypeList = new List<Type>();

        public   Component EdGetComponent(Transform target, Transform root)
        {
            foreach (System.Type t in _CompTypeList)
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
            _CompTypeList.Clear();

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                _BaseViewClass = assembly.GetType(BaseClassName, false, false);
                if (_BaseViewClass != null)
                    break;
            }
            if (_BaseViewClass == null)
            {
                Debug.LogError("找不View的基类 " + BaseClassName);
            }

            UnityEditor.TypeCache.TypeCollection sub_class_list = UnityEditor.TypeCache.GetTypesDerivedFrom<UnityEngine.Component>();
            foreach (var p in CompTypeList)
            {
                bool found = false;
                foreach (var type in sub_class_list)
                {
                    if (type.FullName == p)
                    {
                        _CompTypeList.Add(type);
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