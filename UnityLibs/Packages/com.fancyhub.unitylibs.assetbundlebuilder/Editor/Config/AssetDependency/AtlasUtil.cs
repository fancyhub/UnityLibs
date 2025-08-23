/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 解决 SpriteLoader的问题, 让Sprite 反向依赖Atlas
    /// 这样 加载 Sprite所在的bundle的时候, 也需要加载对应的Atlas的bundle, 当所有依赖atlas的bundle卸载之后, 对应的atlas也会自动卸载
    /// </summary>
    public sealed class AtlasUtil
    {
        public Dictionary<string, string> _Sprite2Atlas = new Dictionary<string, string>();
        public Dictionary<string, List<string>> _Atlas2Atlas = new Dictionary<string, List<string>>();

        public void BuildCache(string[] atlasDirs)
        {
            string[] all_atlas_guids = AssetDatabase.FindAssets("t:SpriteAtlas", atlasDirs);
            bool succ = true;

            foreach (var p in all_atlas_guids)
            {
                string atlas_path = AssetDatabase.GUIDToAssetPath(p);
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlas_path);
                if (atlas.isVariant)
                {
                    _ProcesVariantAtlas(atlas, atlas_path);
                }
                else
                {
                    _Process_Sprites(ref succ, atlas, atlas_path, _Sprite2Atlas);
                }
            }

            if (!succ)
            {
                throw new UnityEditor.Build.BuildFailedException("获取Atlas和Sprite关系的时候出错, 应该有Sprite 被多个Atlas包含");
            }
        }

        public void GetSpriteDependency(string asset_path, List<string> list)
        {
            if (!_Sprite2Atlas.TryGetValue(asset_path, out string atlas_path))
                return;

            list.Add(atlas_path);

            if (_Atlas2Atlas.TryGetValue(atlas_path, out var atlas_list))
            {
                list.AddRange(atlas_list);
            }
        }

        private static void _Process_Sprites(ref bool succ, SpriteAtlas atlas, string atlas_path, Dictionary<string, string> sprite_2_atlas)
        {
            UnityEngine.Object[] packables = atlas.GetPackables();

            foreach (UnityEngine.Object packable in packables)
            {
                string sprite_path = AssetDatabase.GetAssetPath(packable);
                if (packable is Sprite sprite)
                {
                    _Add_Sprite2Atlas(ref succ, sprite_2_atlas, sprite_path, atlas_path);
                }
                else if (packable is Texture texture)
                {
                    Sprite texture_sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprite_path);
                    if (texture_sprite == null)
                        continue;

                    _Add_Sprite2Atlas(ref succ, sprite_2_atlas, sprite_path, atlas_path);
                }

                else if (packable is DefaultAsset default_Asset)
                {
                    var sprite_guids = AssetDatabase.FindAssets("t:Sprite", new string[] { sprite_path });
                    foreach (var p in sprite_guids)
                    {
                        _Add_Sprite2Atlas(ref succ, sprite_2_atlas, AssetDatabase.GUIDToAssetPath(p), atlas_path);
                    }
                }
            }
        }


        private static void _Add_Sprite2Atlas(ref bool succ, Dictionary<string, string> sprite_2_atlas, string sprite_path, string atlas_path)
        {
            if (!sprite_2_atlas.TryGetValue(sprite_path, out var old_atlas_path))
            {
                sprite_2_atlas.Add(sprite_path, atlas_path);
                return;
            }

            if (old_atlas_path == atlas_path)
                return;

            succ = false;
            Debug.LogError($"Sprite {sprite_path} 被多个Atlas包含 \n{old_atlas_path}\n{atlas_path}\n");
        }

        private void _ProcesVariantAtlas(SpriteAtlas variant_atlas, string variant_atlas_path)
        {
            SpriteAtlas main_atlas = variant_atlas.GetMasterAtlas();
            if (main_atlas == null)
                return;

            string main_atlas_path = AssetDatabase.GetAssetPath(main_atlas);
            _Atlas2Atlas.TryGetValue(main_atlas_path, out var list);
            if (list == null)
            {
                list = new List<string>();
                _Atlas2Atlas.Add(main_atlas_path, list);
            }

            list.Add(variant_atlas_path);
        }
    }
}
