using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUIScoller : EdUIComp
    {
        public Vector2 _scroll_pos;
        public override void OnDraw(Vector2 size)
        {
            Rect r = new Rect(Vector2.zero, Tran.size);
            _scroll_pos = GUI.BeginScrollView(r, _scroll_pos, r);
            base.OnDraw(size);
            GUI.EndScrollView();
        }
    }
}
