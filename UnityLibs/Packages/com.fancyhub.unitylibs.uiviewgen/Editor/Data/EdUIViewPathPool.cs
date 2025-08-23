/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 描述了哪些路径 需要处理，运行时候的数据
    /// 主要防止一些prefab重复 生成代码
    /// </summary>
    public class EdUIViewPathPool
    {
        private HashSet<string> _prefab_paths = new();
        private Queue<string> _prefab_paths_todo = new();

        public string Pop()
        {
            if (_prefab_paths_todo.Count == 0)
                return null;
            return _prefab_paths_todo.Dequeue();
        }

        public void Push(string prefab_path)
        {
            if (_prefab_paths.Contains(prefab_path))
                return;
            _prefab_paths.Add(prefab_path);
            _prefab_paths_todo.Enqueue(prefab_path);
        }
    }
}