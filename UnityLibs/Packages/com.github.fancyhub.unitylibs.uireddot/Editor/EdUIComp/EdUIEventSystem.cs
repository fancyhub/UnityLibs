using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUIEventSystem
    {
        public LinkedList<Action> _events = new LinkedList<Action>();
        public void Add(Action cb)
        {
            _events.AddLast(cb);
        }

        public void Process()
        {
            for (var p = _events.First; p != null; p = p.Next)
            {
                try
                {
                    p.Value?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _events.Clear();
        }
    }

}
