using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUIButton : EdUIComp
    {
        public event Action<EdUIButton> OnClick;
        public string Text { get; set; }

        public override void OnDraw(Vector2 size)
        {
            if (GUI.Button(new Rect(Vector2.zero, Tran.size), Text))
            {
                _context.EventSystem.Add(() =>
                    {
                        OnClick?.Invoke(this);
                    });
            }
        }
    }
}
