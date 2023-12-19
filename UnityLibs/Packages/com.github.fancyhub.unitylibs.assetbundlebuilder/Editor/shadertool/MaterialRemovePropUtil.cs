/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/9/3
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 移除Material 里面非法的字段
    /// </summary>
    public class MaterialRemovePropUtil
    {
        private Dictionary<Shader, HashSet<string>> _ShaderKeywordsDict = new Dictionary<Shader, HashSet<string>>();
        public HashSet<string> GetShaderkeywords(Shader shader)
        {
            if (shader == null)
                return null;

            _ShaderKeywordsDict.TryGetValue(shader, out var ret);
            if (ret != null)
                return ret;

            SerializedObject so = new SerializedObject(shader);
            SerializedProperty sp_snippets = so.FindProperty("m_CompileInfo.m_Snippets");
            if (sp_snippets == null || !sp_snippets.isArray)
                return null;

            ret = new HashSet<string>();
            //i看上去和pass的index 有关
            for (int i = 0; i < sp_snippets.arraySize; i++)
            {
                SerializedProperty sp = sp_snippets.GetArrayElementAtIndex(i);
                SerializedProperty sp_second = sp.FindPropertyRelative("second");

                while (sp_second.Next(true))
                {
                    if (!sp_second.propertyPath.Contains("m_VariantsUser"))
                        continue;

                    if (sp_second.propertyType != SerializedPropertyType.String)
                        continue;

                    string key = sp_second.stringValue;
                    if (key == "__" || key == "_")
                        continue;
                    ret.Add(key);
                }
            }

            _ShaderKeywordsDict.Add(shader, ret);
            return ret;
        }

        public bool RemoveUnusedProp(Material mat)
        {
            SerializedObject so = new SerializedObject(mat);
            so.Update();

            int changed_count = 0;
            changed_count += _RemoveUnusedProps(mat, so, "m_SavedProperties.m_TexEnvs");
            changed_count += _RmMissingProps(so, "m_SavedProperties.m_TexEnvs");
            changed_count += _RemoveUnusedProps(mat, so, "m_SavedProperties.m_Floats");
            changed_count += _RemoveUnusedProps(mat, so, "m_SavedProperties.m_Colors");
            if (changed_count > 0)
                so.ApplyModifiedProperties();

            string[] keywords = mat.shaderKeywords;
            List<string> key_words_new = new List<string>();

            Shader shader = mat.shader;
            if (shader != null)
            {
                for (int i = 0; i < keywords.Length; i++)
                {
                    string keyword = keywords[i];
                    if (_ShaderContainKeyWord(mat.shader, keyword))
                    {
                        key_words_new.Add(keyword);
                    }
                }

                if (key_words_new.Count != keywords.Length)
                {
                    mat.shaderKeywords = key_words_new.ToArray();
                    changed_count++;
                }
            }

            return changed_count > 0;
        }

        private int _RmMissingProps(SerializedObject so, string name)
        {
            SerializedProperty props = so.FindProperty(name);
            if (props == null)
                return 0;
            if (!props.isArray)
                return 0;

            int cnt = 0;
            for (int i = 0; i < props.arraySize; i++)
            {
                SerializedProperty sp = props.GetArrayElementAtIndex(i);
                SerializedProperty sp_texture = sp.FindPropertyRelative("second").FindPropertyRelative("m_Texture");
                if (sp_texture.objectReferenceValue == null)
                {
                    sp_texture.objectReferenceValue = null;
                    ++cnt;
                }
            }
            return cnt;
        }

        private int _RemoveUnusedProps(Material mat, SerializedObject so, string name)
        {
            SerializedProperty props = so.FindProperty(name);
            if (props == null) return 0;
            if (!props.isArray) return 0;


            List<int> temp_to_remove = new List<int>();

            for (int i = 0; i < props.arraySize; i++)
            {
                SerializedProperty sp = props.GetArrayElementAtIndex(i);

                string propName = sp.displayName;

                bool exist = mat.HasProperty(propName);
                if (!exist)
                {
                    temp_to_remove.Add(i - temp_to_remove.Count);
                }
            }

            foreach (var p in temp_to_remove)
            {
                props.DeleteArrayElementAtIndex(p);
            }

            return temp_to_remove.Count;
        }


        private bool _ShaderContainKeyWord(Shader shader, string keyword)
        {
            HashSet<string> keys = GetShaderkeywords(shader);
            if (keys == null) 
                return true;
            if (keys.Contains(keyword))
                return true;
            return false;
        }
               

        public void OutputShaderInfo(Shader shader, string file_path)
        {
            SerializedObject so = new SerializedObject(shader);
            SerializedProperty sp = so.GetIterator();
            if (!sp.Next(true))
                return;


            using (FileStream fs = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    do
                    {
                        if (sp.propertyType == SerializedPropertyType.String)
                        {
                            sw.WriteLine(sp.propertyPath + " Value: " + sp.stringValue);
                        }
                    } while (sp.Next(true));
                }
            }
        }
    }
}
