/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

using UnityEngine;
using System.Collections.Generic;

namespace FH.Omi.Editor
{
    public sealed class GroupNode
    {
        public readonly string Name;

        private List<GroupNode> _Children;
        private List<MyMemberInfo> _Members;
        private GroupAttribute _Attribute;
        private string[] _Tabs;
        private Dictionary<string, System.Object> _UserData;

        public GroupNode(string name)
        {
            this.Name = name;
        }

        public T GetUserData<T>(string name)
        {
            if (_UserData == null)
            {
                return default;
            }
            if (!_UserData.TryGetValue(name, out var value))
                return default;

            if (value is T t)
                return t;
            return default;
        }

        public void SetUserData<T>(string name, T data)
        {
            if (_UserData == null)
                _UserData = new Dictionary<string, object>();
            _UserData[name] = data;
        }


        public GroupAttribute GroupAttr => _Attribute;
        public List<MyMemberInfo> Members => _Members;
        public List<GroupNode> Children => _Children;
        public string[] Tabs
        {
            get
            {
                if (_Tabs != null)
                    return _Tabs;

                if (_Children == null || _Children.Count == 0)
                    return null;

                _Tabs = new string[_Children.Count];
                for (int i = 0; i < _Children.Count; i++)
                    _Tabs[i] = _Children[i].Name;
                return _Tabs;
            }
        }


        public void AddChildWithAttribute(GroupAttribute attr)
        {
            var node = _FindNodeByPath(attr.Path, true);
            if (node._Attribute == null)
                node._Attribute = attr;
        }

        public void AddMember(string path, MyMemberInfo member)
        {
            if (member == null)
                return;
            var node = _FindNodeByPath(path, false);
            if (node == null)
            {
                Debug.Log($"Cant find path: {path}");
                return;
            }
            if (node._Members == null)
                node._Members = new List<MyMemberInfo>();
            node._Members.Add(member);
        }

        private GroupNode _FindNodeByPath(string path, bool auto_create)
        {
            if (string.IsNullOrEmpty(path))
                return this;
            var temp = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 0)
                return this;

            GroupNode ret = this;
            foreach (var name in temp)
            {
                GroupNode child = ret._FindChild(name);
                if (child != null)
                {
                    ret = child;
                    continue;
                }

                if (!auto_create)
                    return null;

                child = new GroupNode(name);
                if (ret._Children == null)
                    ret._Children = new List<GroupNode>();
                ret._Children.Add(child);
                ret = child;
            }

            return ret;
        }

        private GroupNode _FindChild(string name)
        {
            if (_Children == null)
                return null;

            foreach (var p in _Children)
            {
                if (p.Name == name)
                    return p;
            }
            return null;
        }
    }
}