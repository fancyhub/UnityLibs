/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using System.Linq;

namespace FH.AssetBundleBuilder.Ed
{
    public static class ShaderTool
    {
        [MenuItem("Tools/AssetBundle/ShaderTool/Collect All Materials", false, 2001)]
        public static void AddAllMaterialsToHelper()
        {
            var config = ShaderDBAsset.LoadOrCreate();

            FileUtil.CreateDir(ShaderDBAsset.CMatDirs);
            List<Material> new_mat_list = _GuidList2MatList(AssetDatabase.FindAssets("t:material", config.SearchMaterialFolders));

            Dictionary<ShaderKey, Material> new_mats = _Convert2Dict(new_mat_list, config.EdGetShaderGroups());
            Dictionary<ShaderKey, Material> old_mats = config.EdGetDict();
            Dictionary<ShaderKey, Material> result_mats = _Merge(old_mats, new_mats);
            config.EdSetDict(result_mats);
            Debug.Log("Success add materials");
        }

        [MenuItem("Tools/AssetBundle/ShaderTool/Remove Material Invalid Props", false, 2001)]
        public static void RemoveMaterialInvalidProps()
        {
            Material mat = Selection.activeObject as Material;
            string path = AssetDatabase.GetAssetPath(mat);
            if (!path.ToLower().EndsWith(".mat"))
                return;
            MaterialRemovePropUtil util = new MaterialRemovePropUtil();
            util.RemoveUnusedProp(mat);
        }
        [MenuItem("Tools/AssetBundle/ShaderTool/Remove Material Invalid Props", true)]
        public static bool RemoveMaterialInvalidProps_Valid()
        {
            Material mat = Selection.activeObject as Material;
            if (mat == null)
                return false;
            string path = AssetDatabase.GetAssetPath(mat);
            if (!path.ToLower().EndsWith(".mat"))
                return false;
            return true; 
        }

        //[MenuItem(MenuItemConfig.C_MAT_ENABLE_MAT_KEYWORDS, false, 1012)]
        public static void EnableLightMapSelectedObjectKeywords()
        {
            GameObject obj = Selection.activeGameObject;
            if (obj == null) return;


            Renderer render = obj.GetComponent<Renderer>();
            if (render == null) return;

            foreach (var m in render.materials)
            {
                m.EnableKeyword("LIGHTMAP_ON");
            }
        }

        private static List<Material> _GuidList2MatList(string[] all_mats_guids)
        {
            List<Material> mats = new List<Material>(all_mats_guids.Length);
            foreach (var p in all_mats_guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(p);
                if (!path.ToLower().EndsWith(".mat"))
                    continue;

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                    continue;
                mats.Add(mat);
            }
            return mats;
        }

        private static Dictionary<ShaderKey, Material> _Convert2Dict(List<Material> mat_list, Dictionary<Shader, List<Shader>> shader_groups)
        {
            Dictionary<ShaderKey, Material> dict = new Dictionary<ShaderKey, Material>(ShaderKey.Empty);
            foreach (var mat in mat_list)
            {
                ShaderKey key = ShaderKey.Create(mat);
                if (key == null)
                    continue;
                if (!dict.ContainsKey(key))
                    dict.Add(key, mat);

                shader_groups.TryGetValue(mat.shader, out var list);
                if (list == null)
                    continue;

                foreach (var s in list)
                {
                    ShaderKey new_key = ShaderKey.Create(key, s);
                    if (new_key == null)
                        continue;
                    if (dict.ContainsKey(new_key))
                        continue;
                    dict.Add(new_key, mat);
                }
            }
            return dict;
        }


        private static Dictionary<ShaderKey, Material> _Merge(
            Dictionary<ShaderKey, Material> old_mats,
            Dictionary<ShaderKey, Material> new_mats)
        {
            Dictionary<ShaderKey, Material> ret = new Dictionary<ShaderKey, Material>(ShaderKey.Empty);

            ShaderTexture shaderTexture = ShaderTexture.Load();

            System.Text.StringBuilder sb = new StringBuilder();
            //1. 删除多余的
            foreach (var p in old_mats)
            {
                if (new_mats.ContainsKey(p.Key))
                {
                    ret.Add(p.Key, p.Value);
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(p.Value);
                    AssetDatabase.DeleteAsset(path);
                }
            }

            //2. 添加不存在的
            foreach (var p in new_mats)
            {
                if (ret.ContainsKey(p.Key))
                    continue;

                Material mat = _CloneMat(p.Value, p.Key.Shader, shaderTexture);

                //保存

                string name = mat.shader.name.Replace("/", ".");
                string guid = System.Guid.NewGuid().ToString().Replace("-", "");
                string new_path = $"{ShaderDBAsset.CMatDirs}/{name}_{guid}.mat";
                AssetDatabase.CreateAsset(mat, new_path);


                {
                    ShaderKey new_key = ShaderKey.Create(mat);
                    if (!new_key.Equals(p.Key))
                    {
                        sb.Clear();
                        sb.AppendLine("Key 不相同");

                        sb.AppendLine($"old:{AssetDatabase.GetAssetPath(p.Value)}");
                        sb.Append($"\t\tKeys{p.Key.Keys.Length}:");
                        foreach (var k in p.Key.Keys)
                        {
                            sb.Append(k);
                            sb.Append(" ");
                        }
                        sb.AppendLine();

                        sb.AppendLine($"new:{new_path}");
                        sb.Append($"\t\tKeys{new_key.Keys.Length}:");
                        foreach (var k in new_key.Keys)
                        {
                            sb.Append(k);
                            sb.Append(" ");
                        }
                        sb.AppendLine();
                        sb.AppendLine();

                        UnityEngine.Debug.Assert(false, sb.ToString());
                    }

                    ret.Add(p.Key, mat);
                }
            }

            return ret;
        }

        private static UnityEngine.Object[] _S_TempMatArrary = new Object[1];
        public static Material _CloneMat(Material old_mat, Shader shader, ShaderTexture shader_texture)
        {
            Material mat_new = new Material(old_mat.shader);
            mat_new.enabledKeywords = old_mat.enabledKeywords;
            //foreach (var a in old_mat.shaderKeywords)
            //{
            //    mat_new.EnableKeyword(a);
            //}


            _S_TempMatArrary[0] = old_mat;
            MaterialProperty[] props = MaterialEditor.GetMaterialProperties(_S_TempMatArrary);
            mat_new.globalIlluminationFlags = old_mat.globalIlluminationFlags;

            foreach (var pro in props)
            {
                switch (pro.type)
                {
                    case MaterialProperty.PropType.Color:
                        mat_new.SetColor(pro.name, pro.colorValue);
                        break;

                    case MaterialProperty.PropType.Vector:
                        mat_new.SetVector(pro.name, pro.vectorValue);
                        break;

                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        mat_new.SetFloat(pro.name, pro.floatValue);
                        break;

                    case MaterialProperty.PropType.Int:
                        mat_new.SetFloat(pro.name, pro.intValue);
                        break;


                    case MaterialProperty.PropType.Texture:
                        Texture tex = pro.textureValue;
                        if (tex == null)
                            break;

                        if (tex is Texture2D)
                        {
                            string pro_name = pro.name.ToLower();
                            if (pro_name.Contains("norm") || pro_name.Contains("bump"))
                            {
                                mat_new.SetTexture(pro.name, shader_texture.Simple2DNormalMap);
                            }
                            else
                            {
                                mat_new.SetTexture(pro.name, shader_texture.Simple2D);
                            }
                        }
                        else if (tex is Cubemap)
                        {
                            mat_new.SetTexture(pro.name, shader_texture.SimpleCubemap);
                        }
                        else if (tex is Texture3D)
                        {
                            mat_new.SetTexture(pro.name, shader_texture.Simple3D);
                        }
                        else if (tex is Texture2DArray)
                        {
                            mat_new.SetTexture(pro.name, shader_texture.Simple2DArray);
                        }
                        else if (tex is CubemapArray)
                        {
                            mat_new.SetTexture(pro.name, shader_texture.SimpleCubemap);
                        }
                        break;
                }
            }

            mat_new.shader = shader;
            return mat_new;
        }
    }
}
