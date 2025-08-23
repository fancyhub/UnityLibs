using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUICanvas : EdUIComp
    {
        public EdUICanvas()
        {
            _context = new EdUIContext();
        }

        public void Update(Vector2 size)
        {
            Draw(size);
            _context.EventSystem.Process();
        }
    }
}
