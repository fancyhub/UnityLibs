/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FH.AssetBundleBuilder.Ed
{
    [Serializable]
    public class ShaderDBAsset : ScriptableObject
    {
        private const string CPath = "Assets/fancyhub/ShaderDB.asset";
        public const string CMatDirs = "Assets/fancyhub/ShaderMats";

        public string[] SearchMaterialFolders = new string[] { };

        public static ShaderDBAsset Load()
        {
            return AssetDatabase.LoadAssetAtPath<ShaderDBAsset>(CPath); 
        }

        public static ShaderDBAsset LoadOrCreate()
        {
            ShaderDBAsset db = AssetDatabase.LoadAssetAtPath<ShaderDBAsset>(CPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ShaderDBAsset>();
                FileUtil.CreateFileDir(CPath);
                AssetDatabase.CreateAsset(db, CPath);
            }
            return db;
        }

        public List<ShaderGroup> ShaderGroups = new List<ShaderGroup>();
        public List<ShaderItem> ShaderList = new List<ShaderItem>();

        [Serializable]
        public class ShaderItem
        {
            public Shader Shader;
            public List<ShaderMaterialItem> MaterialList = new List<ShaderMaterialItem>();
        }

        [Serializable]
        public class ShaderMaterialItem
        {
            public Material Material;
            public string[] Keys;

            public ShaderMaterialItem(Material mat, string[] keys)
            {
                this.Material = mat;
                this.Keys = keys;
            }
        }

        /// <summary>
        /// 有些 Material 在运行的时候, 会动态切换Shader, 但是里面的Keys是一样的
        /// 但是平时是不会制作这样的Material,收集Keys的时候会遗漏, 所以需要额外的配置
        /// </summary>
        [Serializable]
        public class ShaderGroup
        {
            public Shader MainShader;
            public List<Shader> Shaders = new List<Shader>();
        }


        public Dictionary<string, List<string>> EdGetShaderMaterialDict()
        {
            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
            foreach (var p in ShaderList)
            {
                if (p.Shader == null)
                    continue;
                string shader_path = AssetDatabase.GetAssetPath(p.Shader);
                List<string> mats = new List<string>();
                ret.Add(shader_path, mats);
                foreach (var p2 in p.MaterialList)
                {
                    if (p2.Material == null)
                        continue;
                    mats.Add(AssetDatabase.GetAssetPath(p2.Material));
                }
            }
            return ret;
        }

        public void EdSetDict(Dictionary<ShaderKey, Material> dict)
        {
            Dictionary<Shader, ShaderItem> shader_dict = new Dictionary<Shader, ShaderItem>();
            foreach (var p in dict)
            {
                shader_dict.TryGetValue(p.Key.Shader, out var shader_item);
                if (shader_item == null)
                {
                    shader_item = new ShaderItem();
                    shader_item.Shader = p.Key.Shader;
                    shader_dict.Add(p.Key.Shader, shader_item);
                }
                shader_item.MaterialList.Add(new ShaderMaterialItem(p.Value, p.Key.Keys));
            }

            ShaderList.Clear();
            foreach (var p in shader_dict)
            {
                ShaderList.Add(p.Value);
            }

            EditorUtility.SetDirty(this);
        }

        public Dictionary<Shader, List<Shader>> EdGetShaderGroups()
        {
            Dictionary<Shader, List<Shader>> ret = new Dictionary<Shader, List<Shader>>();

            foreach (var p in ShaderGroups)
            {
                if (p.MainShader == null)
                    continue;
                ret[p.MainShader] = p.Shaders;
            }
            return ret;
        }

        public Dictionary<ShaderKey, Material> EdGetDict()
        {
            Dictionary<ShaderKey, Material> ret = new Dictionary<ShaderKey, Material>(ShaderKey.Empty);
            foreach (var p in ShaderList)
            {
                for (int i = p.MaterialList.Count - 1; i >= 0; i--)
                {
                    var mat = p.MaterialList[i];
                    if (mat == null)
                    {
                        p.MaterialList.RemoveAt(i);
                        continue;
                    }

                    ShaderKey key = ShaderKey.Create(mat.Material);
                    if (key == null || ret.ContainsKey(key))
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mat.Material));
                        p.MaterialList.RemoveAt(i);
                        continue;
                    }
                    ret.Add(key, mat.Material);
                }
            }

            return ret;
        }
    }
}
