/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    //最终要给mapnode 对应一个asset_bundle
    public class BundleNode : IEnumerable<AssetObj>
    {
        public const string CIgnoreAddressName = " ";
        private static int S_NODE_ID_GEN = 0;

        public readonly int Id;

        private readonly string _Name = null;
        private string _NameWithGuid = null;

        public bool _IsSceneNode = false;

        public List<AssetObj> _main_objs = new List<AssetObj>();
        public List<AssetObj> _dep_objs = new List<AssetObj>();
        public HashSet<BundleNode> _dep_nodes = new HashSet<BundleNode>();

        //运行时候需要处理的
        private bool _SortDirty = true;

        public bool FlagNeedProcess { get; set; } = true;

        public BundleNode(string name)
        {
            _Name = name;
            Id = S_NODE_ID_GEN++;
        }

        public bool IsEmpty()
        {
            if (_main_objs.Count > 0)
                return false;
            if (_dep_objs.Count > 0)
                return false;
            return true;
        }

        public List<AssetObj> GetMainObjs()
        {
            return _main_objs;
        }

        public void AddMainObj(AssetObj obj)
        {
            if (_NameWithGuid != null)
            {
                BuilderLog.Error("名字已经创建了，不能添加新节点了");
                return;
            }

            _SortDirty = true;
            _main_objs.Add(obj);
        }

        public void AddDepObj(AssetObj obj)
        {
            if (_NameWithGuid != null)
            {
                BuilderLog.Error("名字已经创建了，不能添加新节点了");
                return;
            }

            _SortDirty = true;
            _dep_objs.Add(obj);
        }

        public bool RemoveDepObj(AssetObj obj)
        {
            _SortDirty = true;
            return _dep_objs.Remove(obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public bool IsSceneNode()
        {
            return _IsSceneNode;
        }

        public bool IsObjInDepNodes(AssetObj obj)
        {
            foreach (var a in _dep_nodes)
            {
                if (a._dep_objs.Contains(obj))
                    return true;

                if (a.IsObjInDepNodes(obj))
                    return true;
            }
            return false;
        }

        public UnityEditor.AssetBundleBuild GenAssetBundleBuild()
        {
            UnityEditor.AssetBundleBuild ret = new UnityEditor.AssetBundleBuild();
            ret.assetBundleName = GetNodeName();
            ret.assetNames = _GetAllFiles().ToArray();
            ret.addressableNames = _GetAllAddressNames().ToArray();
            return ret;
        }

        public List<AssetObj> GetDepObjs()
        {
            return _dep_objs;
        }


        public HashSet<BundleNode> GetDepNodes()
        {
            return _dep_nodes;
        }

        public override string ToString()
        {
            if (null == _Name)
                return base.ToString();
            return _Name;
        }

        public string GetNodeName()
        {
            if (null != _Name)
                return _Name;

            if (null != _NameWithGuid)
                return _NameWithGuid;

            _SortAssets();
            //在 _main_objs 里面找到最小的 guid作为名字
            if (_main_objs.Count > 0)
            {
                _NameWithGuid = "u_" + _main_objs[0].GUID.ToLower();
            }
            else if (_dep_objs.Count > 0)
            {
                _NameWithGuid = "u_" + _dep_objs[0].GUID.ToLower();
            }
            return _NameWithGuid;
        }

        private List<string> _GetAllFiles()
        {
            _SortAssets();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObj a in _main_objs)
            {
                file_list.Add(a.FilePath);
            }

            foreach (AssetObj a in _dep_objs)
            {
                file_list.Add(a.FilePath);
            }
            return file_list;
        }

        private List<string> _GetAllAddressNames()
        {
            _SortAssets();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObj a in _main_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a.AddressName);
            }

            foreach (AssetObj a in _dep_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a.AddressName);
            }
            return file_list;
        }

        public bool HasObject(AssetObj obj)
        {
            if (_main_objs.Contains(obj))
                return true;
            if (_dep_objs.Contains(obj))
                return true;
            return false;
        }

        private void _SortAssets()
        {
            if (!_SortDirty)
                return;

            _SortDirty = false;
            _main_objs.Sort(_Compare);
            _dep_objs.Sort(_Compare);
        }

        private static int _Compare(AssetObj a, AssetObj b)
        {
            return a.GUID.CompareTo(b.GUID);
        }

        public IEnumerator<AssetObj> GetEnumerator()
        {
            foreach (var a in _main_objs)
                yield return a;
            foreach (var a in _dep_objs)
                yield return a;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 找到所有 node1 依赖 node2 的文件列表，
        /// key 是 node1 里面的文件
        /// value 是 node2 里面的文件
        /// 
        /// 只处理main 和main之间的关系
        /// </summary>        
        public List<KeyValuePair<string, string>> FindDep(BundleNode node2)
        {
            List<KeyValuePair<string, string>> ret = new List<KeyValuePair<string, string>>();
            foreach (var a in _main_objs)
            {                
                foreach (var b in a.GetDepObjs())
                {
                    if (node2._main_objs.Contains(b))
                        ret.Add(new KeyValuePair<string, string>(a.FilePath, b.FilePath));
                }
            }
            return ret;
        }
    }
}
