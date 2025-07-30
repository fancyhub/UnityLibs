/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/8 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public static class LinkedListExt
    {
        public class LinkedListCache<T>
        {
            public static LinkedList<T> _cache = new LinkedList<T>();
        }

        public static LinkedListNode<T> ExtAddBefore<T>(this LinkedList<T> list, LinkedListNode<T> node, T val)
        {
            if (list == null)
            {
                // my.assert(false, "list 为空");
                return null;
            }
            if (node != null && node.List != list)
            {
                // my.assert(false, "node 不是list的节点");
                return null;
            }

            //如果节点为空，添加到第一个节点
            if (node == null)
            {
                return ExtAddLast(list, val);
            }

            //获取新节点
            LinkedList<T> cache_list = LinkedListCache<T>._cache;
            LinkedListNode<T> new_node = cache_list.First;
            if (null == new_node)
                new_node = new LinkedListNode<T>(val);
            else
            {
                new_node.Value = val;
                cache_list.Remove(new_node);
            }

            list.AddBefore(node, new_node);
            return new_node;
        }

        public static LinkedListNode<T> ExtAddAfter<T>(this LinkedList<T> list, LinkedListNode<T> node, T val)
        {
            if (list == null)
            {
                // my.assert(false, "list 为空");
                return null;
            }
            if (node != null && node.List != list)
            {
                // my.assert(false, "node 不是list的节点");
                return null;
            }

            //如果节点为空，添加到第一个节点
            if (node == null)
            {
                return ExtAddLast(list, val);
            }

            //获取新节点
            LinkedList<T> cache_list = LinkedListCache<T>._cache;
            LinkedListNode<T> new_node = cache_list.First;
            if (null == new_node)
                new_node = new LinkedListNode<T>(val);
            else
            {
                new_node.Value = val;
                cache_list.Remove(new_node);
            }

            list.AddAfter(node, new_node);
            return new_node;
        }

        public static bool ExtMove2First<T>(this LinkedList<T> list, LinkedListNode<T> node)
        {
            if (null == node)
                return false;
            if (node.List != list)
                return false;
            if (node == list.First) return true;
            list.Remove(node);
            list.AddFirst(node);
            return true;
        }

        public static bool ExtMove2First<T>(this LinkedListNode<T> node)
        {
            if (node == null) return false;
            if (node.List == null) return false;

            var list = node.List;
            if (node == list.First) return true;

            list.Remove(node);
            list.AddFirst(node);
            return true;
        }

        public static bool ExtMove2Last<T>(this LinkedList<T> list, LinkedListNode<T> node)
        {
            if (null == node)
                return false;
            if (node.List != list)
                return false;
            if (node == list.Last) return true;
            list.Remove(node);
            list.AddLast(node);
            return true;
        }

        public static bool ExtMove2Last<T>(this LinkedListNode<T> node)
        {
            if (node == null) return false;
            if (node.List == null) return false;

            var list = node.List;
            if (node == list.First) return true;

            list.Remove(node);
            list.AddLast(node);
            return true;
        }

        public static void ExtClear<T>(this LinkedList<T> list)
        {
            if (list == null)
                return;

            LinkedList<T> cache_list = LinkedListCache<T>._cache;
            LinkedListNode<T> node = list.First;
            for (; ; )
            {
                if (node == null)
                    break;
                LinkedListNode<T> next = node.Next;

                node.Value = default(T);
                list.Remove(node);
                cache_list.AddLast(node);
                node = next;
            }
        }

        public static bool ExtRemoveFirst<T>(this LinkedList<T> list)
        {
            if (list == null)
                return false;
            var node = list.First;
            if (node == null)
                return false;

            list.Remove(node);
            node.Value = default;
            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }

        public static bool ExtRemoveFromList<T>(this LinkedListNode<T> node)
        {
            if (node == null) return false;
            node.Value = default(T);

            if (node.List != null)
                node.List.Remove(node);

            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }


        public static bool ExtRemove<T>(this LinkedList<T> list, LinkedListNode<T> node)
        {
            if (node == null) return false;
            if (node.List != list) return false;

            node.Value = default(T);
            list.Remove(node);
            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }

        public static bool ExtRemove<T>(this LinkedList<T> list, T val)
        {
            LinkedListNode<T> node = list.Find(val);
            if (node == null) return false;

            node.Value = default(T);
            list.Remove(node);
            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }

        public static bool ExtPeekFirst<T>(this LinkedList<T> list, out T val)
        {
            if (list == null)
            {
                val = default(T);
                return false;
            }

            LinkedListNode<T> node = list.First;
            if (node == null)
            {
                val = default(T);
                return false;
            }
            val = node.Value;
            return true;
        }

        public static bool ExtPopFirst<T>(this LinkedList<T> list, out T val)
        {
            if (list == null)
            {
                val = default(T);
                return false;
            }

            LinkedListNode<T> node = list.First;
            if (node == null)
            {
                val = default(T);
                return false;
            }
            val = node.Value;

            node.Value = default(T);
            list.Remove(node);
            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }
 
        public static bool ExtPopFirstNode<T>(this LinkedList<T> list, out LinkedListNode<T> node)
        {
            if (list == null)
            {
                node = null;
                return false;
            }

            node = list.First;
            if (node == null)
                return false;
            list.Remove(node);
            return true;
        }

        public static bool ExtPopLast<T>(this LinkedList<T> list, out T val)
        {
            if (list == null)
            {
                val = default;
                return false;
            }

            LinkedListNode<T> node = list.Last;
            if (node == null)
            {
                val = default(T);
                return false;
            }
            val = node.Value;

            node.Value = default(T);
            list.Remove(node);
            LinkedListCache<T>._cache.AddLast(node);
            return true;
        }

        public static LinkedListNode<T> ExtAddLast<T>(this LinkedList<T> list, T val)
        {
            LinkedList<T> cache_list = LinkedListCache<T>._cache;
            LinkedListNode<T> node = cache_list.First;
            if (null == node)
                node = new LinkedListNode<T>(val);
            else
            {
                node.Value = val;
                cache_list.Remove(node);
            }
            list.AddLast(node);
            return node;
        }

        public static LinkedListNode<T> ExtAddFirst<T>(this LinkedList<T> list, T val)
        {
            LinkedList<T> cache_list = LinkedListCache<T>._cache;
            LinkedListNode<T> node = cache_list.First;
            if (null == node)
                node = new LinkedListNode<T>(val);
            else
            {
                node.Value = val;
                cache_list.Remove(node);
            }
            list.AddFirst(node);
            return node;
        }
    }
}