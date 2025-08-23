using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUITexture : EdUIComp
    {
        public Texture Texture { get; set; }
        public ScaleMode ScaleMode { get; set; }
        public bool AlphaBlend { get; set; }
        public Color Color { get; set; }

        public EdUITexture()
        {
            Texture = Texture2D.whiteTexture;
            Color = Color.white;
        }

        public override void OnDraw(Vector2 size)
        {
            Rect rect = new Rect();
            rect.size = size;
            GUI.DrawTexture(rect, Texture, ScaleMode, AlphaBlend, 0, Color, 0, 0);
        }
    }
}
