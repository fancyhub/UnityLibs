using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 检查 一个Sprite 不能出现在两个 Atlas里面
    /// </summary>
    public class AtlasChecker
    {
        public void Check(AssetObjMap asset_map)
        {
            Dictionary<AssetObj, string> ret = new Dictionary<AssetObj, string>();

            //1. 获取所有的 atlas
            List<AssetObj> all_atlas = asset_map.FindObjects(EAssetObjType.atlas);

            //2. 建立 atlas 之间的联系,  解决变体的问题, 依赖关系要倒过来
            Dictionary<AssetObj, List<AssetObj>> atlas_2_atlas = new Dictionary<AssetObj, List<AssetObj>>();
            foreach (var p in all_atlas)
            {
                AssetObj sub_obj = null;
                foreach (var sub_p in p.GetDepObjs())
                {
                    if (sub_p.AssetType == EAssetObjType.atlas)
                    {
                        sub_obj = sub_p;
                        break;
                    }
                }

                if (sub_obj == null)
                {
                    //说明自己是顶级的
                    atlas_2_atlas.TryGetValue(p, out var parent_list);
                    if (parent_list == null)
                    {
                        parent_list = new List<AssetObj>();
                        atlas_2_atlas.Add(p, parent_list);
                    }
                }
                else
                {
                    //说明自己要合并到依赖的Atlas包里面, 因为Atlas另外一个atlas变体的变体,不会出现 A-> ACopy1 -> ACopy12 这种关系
                    atlas_2_atlas.TryGetValue(sub_obj, out var parent_list);
                    if (parent_list == null)
                    {
                        parent_list = new List<AssetObj>();
                        atlas_2_atlas.Add(sub_obj, parent_list);
                    }
                    parent_list.Add(p);
                }
            }

            HashSet<AssetObj> temp = new HashSet<AssetObj>();
            //3. 收集 Atlas所有依赖的文件, 添加到到ret里面
            foreach (var p in atlas_2_atlas)
            {
                //3.1 生成包的名字
                string ab_name = System.IO.Path.GetFileNameWithoutExtension(p.Key.FilePath).ToLower();
                ab_name = "a_" + ab_name;

                //3.2 把atlas 加到包里
                ret.Add(p.Key, ab_name);
                foreach (var sub_atals in p.Value)
                {
                    ret.Add(sub_atals, ab_name);
                }

                //3.3 把依赖的sprite 加到包里面
                temp.Clear();
                p.Key.GetAllDepObjs(temp);

                foreach (var texture in temp)
                {
                    if (texture.AssetType != EAssetObjType.texture)
                        continue;

                    if (ret.ContainsKey(texture))
                    {
                        throw new Exception(string.Format("Sprite {0} 被两个atlas 包含了", texture.FilePath));
                    }
                    ret.Add(texture, ab_name);
                }
            }            
        }
    }
}
