/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/

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
    }

    public interface IVFSMgr : ICPtr
    {
        public void Mount(IVirtualFileSystem file_system);

        public List<IVirtualFileSystem> GetAll();

        public byte[] ReadAllBytes(string file_path);

        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>        
        public Stream OpenRead(string file_path);
    }
}
