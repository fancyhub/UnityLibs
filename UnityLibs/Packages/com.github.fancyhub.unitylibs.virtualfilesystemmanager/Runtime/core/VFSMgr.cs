/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.VFSManagement;
using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{

    public interface IVirtualFileSystem : IDestroyable
    {
        public string Name { get; }

        public byte[] ReadAllBytes(string file_path);

        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>        
        public Stream OpenRead(string file_path);

        public bool Exist(string file_path);
    }

    public partial interface IVfsMgr : ICPtr
    {

        public IVirtualFileSystem FindFileSystem(string file_system_name);

        public bool Mount(IVirtualFileSystem file_system);

        public bool UnMount(string file_system_name);
        public void UnMountAll();

        public List<IVirtualFileSystem> GetAll();

        public byte[] ReadAllBytes(string file_path);

        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>        
        public Stream OpenRead(string file_path);
    }


    public static class VfsMgr
    {
        private static CPtr<IVfsMgr> _;

        public static void InitMgr(IVfsMgr.Config config)
        {
            if (_.Val != null)
            {
                VfsLog._.E("VfsMgr Is Inited, can't create twice");
                return;
            }

            if (config == null)
            {
                VfsLog._.E("config is null");
                return;
            }

            VfsLog._ = TagLogger.Create(VfsLog._.Tag, config.LogLvl);

            VfsMgrImplement mgr_imp = new VfsMgrImplement();
            _ = new CPtr<IVfsMgr>(mgr_imp);
        }

        public static bool Mount(IVirtualFileSystem file_system)
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return false;
            }

            return mgr.Mount(file_system);
        }

        public static bool UnMount(string file_system_name)
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return false;
            }

            return mgr.UnMount(file_system_name);
        }

        public static void UnMountAll()
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return;
            }
            mgr.UnMountAll();
        }

        public static List<IVirtualFileSystem> GetAll()
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return null;
            }

            return mgr.GetAll();
        }

        public static byte[] ReadAllBytes(string file_path)
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return null;
            }

            return mgr.ReadAllBytes(file_path);
        }

        public static string ReadAllText(string file_path)
        {
            byte[] ret = ReadAllBytes(file_path);
            if (null == ret)
                return null;
            return System.Text.Encoding.UTF8.GetString(ret);
        }


        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>                
        public static Stream OpenRead(string file_path)
        {
            IVfsMgr mgr = _.Val;
            if (mgr == null)
            {
                VfsLog._.E("VfsMgr Is Null");
                return null;
            }

            return mgr.OpenRead(file_path);
        }
    }
}