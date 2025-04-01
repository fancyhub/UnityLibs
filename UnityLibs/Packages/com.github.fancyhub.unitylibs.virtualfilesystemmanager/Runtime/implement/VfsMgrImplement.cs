/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace FH.VFSManagement
{
    internal sealed class VfsMgrImplement : IVfsMgr
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public List<IVirtualFileSystem> _file_system_list = new List<IVirtualFileSystem>();

        public void Destroy()
        {
            ___obj_ver++;
            UnMountAll();
        }

        public List<IVirtualFileSystem> GetAll()
        {
            return _file_system_list;
        }

        public IVirtualFileSystem FindFileSystem(string file_system_name)
        {
            int index = _IndexOf(file_system_name);
            if (index < 0)
                return null;
            return _file_system_list[index];
        }

        public bool Mount(IVirtualFileSystem file_system)
        {
            if (null == file_system)
            {
                VfsLog._.E("空对象");
                return false;
            }

            //已经存在了
            if (FindFileSystem(file_system.Name) != null)
            {
                VfsLog._.E("已经存在了 {0}", file_system.Name);
                return false;
            }

            _file_system_list.Add(file_system);
            VfsLog._.D("Mount {0} ", file_system.Name);
            return true;
        }

        public bool UnMount(string file_system_name)
        {
            int index = _IndexOf(file_system_name);
            if (index < 0)
                return false;
            var fs = _file_system_list[index];

            VfsLog._.D("Unmount {0} ", fs.Name);
            _file_system_list.RemoveAt(index);
            fs.Destroy();
            return true;
        }
        public void UnMountAll()
        {
            VfsLog._.D("Unmount All {0} ", _file_system_list.Count);

            foreach (var p in _file_system_list)
            {
                p.Destroy();
            }
            _file_system_list.Clear();
        }

        public Stream OpenRead(string file_path)
        {
            VfsLog._.D("ReadStream {0}", file_path);
            for (int i = 0; i < _file_system_list.Count; ++i)
            {
                IVirtualFileSystem sub_system = _file_system_list[i];
                Stream ret = sub_system.OpenRead(file_path);
                if (null != ret)
                    return ret;
            }

            VfsLog._.E("找不到  {0}", file_path);
            return null;
        }

        public string ReadAllText(string file_path)
        {
            VfsLog._.D("ReadAllBytes {0}", file_path);

            for (int i = 0; i < _file_system_list.Count; ++i)
            {
                IVirtualFileSystem sub_system = _file_system_list[i];
                string ret = sub_system.ReadAllText(file_path);
                if (null != ret)
                    return ret;
            }
            VfsLog._.E("找不到  {0}", file_path);
            return null;
        }

        public byte[] ReadAllBytes(string file_path)
        {
            VfsLog._.D("ReadAllBytes {0}", file_path);

            for (int i = 0; i < _file_system_list.Count; ++i)
            {
                IVirtualFileSystem sub_system = _file_system_list[i];
                byte[] ret = sub_system.ReadAllBytes(file_path);
                if (null != ret)
                    return ret;
            }
            VfsLog._.E("找不到  {0}", file_path);
            return null;
        }

        private int _IndexOf(string file_system_name)
        {
            for (int i = 0; i < _file_system_list.Count; i++)
            {
                if (_file_system_list[i].Name == file_system_name)
                    return i;
            }
            return -1;
        }

        
    }
}
