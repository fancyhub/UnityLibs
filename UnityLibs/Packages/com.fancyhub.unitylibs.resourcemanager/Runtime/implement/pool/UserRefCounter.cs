/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR
#define USER_OBJ
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ResManagement
{
    internal class UserRefCounter : IDestroyable
    {
        public int _user_count;
#if USER_OBJ
        public LinkedList<System.Object> _user_list;
#endif

        public UserRefCounter()
        {
            _user_count = 0;
#if USER_OBJ
            _user_list = new LinkedList<object>();
#endif
        }

        public bool AddUser(System.Object user)
        {
            if (user == null)
                return false;
#if USER_OBJ
            if (_user_list != null)
            {
                if (_user_list.Contains(user))
                    return false;
                _user_list.ExtAddLast(user);
            }
#endif
            _user_count++;
            return true;
        }

        public bool RemoveUser(System.Object user)
        {
            if (user == null)
                return false;
#if USER_OBJ
            if (_user_list != null)
            {
                LinkedListNode<System.Object> node = _user_list.Find(user);
                if (node == null)
                    return false;
                _user_list.ExtRemove(node);
            }
#endif
            _user_count--;
            if (_user_count < 0)
                _user_count = 0;
            return true;
        }

        public int GetUserCount()
        {
            return _user_count;
        }

        public virtual void Destroy()
        {
#if USER_OBJ
            _user_list.ExtClear();
#endif
            _user_count = 0;
        }

        public void CopyUserList(ref List<object> out_list)
        {
#if USER_OBJ
            if (out_list == null)
            {
                out_list = new List<object>(_user_list.Count);
            }
            out_list.AddRange(out_list);
#else 
            if(out_list == null)
            {
                out_list = new List<object>(0);
            }
#endif
        }
    }
}
