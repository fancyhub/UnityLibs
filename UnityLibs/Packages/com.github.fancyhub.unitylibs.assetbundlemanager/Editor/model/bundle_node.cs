using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17 14:10:55
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.AssetBundleManager.Builder
{
    //最终要给mapnode 对应一个asset_bundle
    public class BundleNode : IEnumerable<AssetObject>
    {
        public const string CIgnoreAddressName = " ";

        private static int S_NODE_ID_GEN = 0;
        public int _id;
        readonly int _hash_code = 0;
        public string _name = null;
        public string _name_guid = null;
        public bool _is_scene_node = false;

        public List<AssetObject> _main_objs = new List<AssetObject>();
        public List<AssetObject> _dep_objs = new List<AssetObject>();
        public HashSet<BundleNode> _dep_nodes = new HashSet<BundleNode>();

        //运行时候需要处理的
        public bool _sort_dirty = true;
        public bool _flag_need_process = true;

        public BundleNode(string name)
        {
            _name = name;
            _id = S_NODE_ID_GEN;
            _hash_code = S_NODE_ID_GEN.GetHashCode();
            S_NODE_ID_GEN++;
        }

        public bool IsEmpty()
        {
            if (_main_objs.Count > 0)
                return false;
            if (_dep_objs.Count > 0)
                return false;
            return true;
        }

        public List<AssetObject> GetMainObjs()
        {
            return _main_objs;
        }

        public void AddMainObj(AssetObject obj)
        {
            if (_name_guid != null)
                BuilderLog.Error("名字已经创建了，不能添加新节点了");
            _sort_dirty = true;
            _main_objs.Add(obj);
        }

        public void AddDepObj(AssetObject obj)
        {
            if (_name_guid != null)
                BuilderLog.Error("名字已经创建了，不能添加新节点了");
            _sort_dirty = true;
            _dep_objs.Add(obj);
        }

        public bool RemoveDepObj(AssetObject obj)
        {
            _sort_dirty = true;
            return _dep_objs.Remove(obj);
        }

        public override int GetHashCode()
        {
            return _hash_code;
        }

        public bool IsSceneNode()
        {
            return _is_scene_node;
        }

        public bool IsObjInDepNodes(AssetObject obj)
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

        public AssetBundleBuild GenAssetBundleBuild()
        {
            AssetBundleBuild ret = new AssetBundleBuild();
            ret.assetBundleName = GetNodeName();
            ret.assetNames = _GetAllFiles().ToArray();
            ret.addressableNames = _GetAllAddressNames().ToArray();
            return ret;
        }

        public List<AssetObject> GetDepObjs()
        {
            return _dep_objs;
        }


        public HashSet<BundleNode> GetDepNodes()
        {
            return _dep_nodes;
        }

        public override string ToString()
        {
            if (null == _name)
                return base.ToString();
            return _name;
        }

        public string GetNodeName()
        {
            if (null != _name)
                return _name;

            if (null != _name_guid)
                return _name_guid;

            _sort();
            //在 _main_objs 里面找到最小的 guid作为名字
            if (_main_objs.Count > 0)
            {
                _name_guid = "u_" + _main_objs[0]._guid.ToLower();
            }
            else if (_dep_objs.Count > 0)
            {
                _name_guid = "u_" + _dep_objs[0]._guid.ToLower();
            }
            return _name_guid;
        }

        private List<string> _GetAllFiles()
        {
            _sort();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObject a in _main_objs)
            {
                file_list.Add(a._file_path);
            }

            foreach (AssetObject a in _dep_objs)
            {
                file_list.Add(a._file_path);
            }
            return file_list;
        }

        private List<string> _GetAllAddressNames()
        {
            _sort();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObject a in _main_objs)
            {
                if (!a._need_export)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a._address_name);                
            }

            foreach (AssetObject a in _dep_objs)
            {
                if (!a._need_export)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a._address_name);
            }
            return file_list;
        }

        public bool HasObject(AssetObject obj)
        {
            if (_main_objs.Contains(obj))
                return true;
            if (_dep_objs.Contains(obj))
                return true;
            return false;
        }

        private void _sort()
        {
            if (!_sort_dirty)
                return;

            _sort_dirty = false;
            _main_objs.Sort(_compare);
            _dep_objs.Sort(_compare);
        }

        private int _compare(AssetObject a, AssetObject b)
        {
            return a._guid.CompareTo(b._guid);
        }

        public IEnumerator<AssetObject> GetEnumerator()
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
                if (a._dep_objs == null)
                    continue;

                foreach (var b in a._dep_objs)
                {
                    if (node2._main_objs.Contains(b))
                        ret.Add(new KeyValuePair<string, string>(a._file_path, b._file_path));
                }
            }
            return ret;
        }
    }
}
