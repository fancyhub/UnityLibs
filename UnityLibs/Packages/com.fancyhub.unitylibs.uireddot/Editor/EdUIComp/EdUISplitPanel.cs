using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI.Ed
{
    public class EdUISplitPanel : EdUIComp
    {
        public enum E_DIR
        {
            Horizontal,
            Vertical,
        }
        public E_DIR Dir { get; set; }
        public EdUIComp[] _sub_panels;
        public float _split_pos = 0.5f;
        private bool _resize_flag = false;

        public EdUISplitPanel()
        {
            _sub_panels = new EdUIComp[2];
        }

        public override bool AddChild(EdUIComp child)
        {
            return false;
        }


        public float SplitPos { get { return _split_pos; } set { _split_pos = Mathf.Clamp01(value); } }

        public override bool RemoveChild(EdUIComp child)
        {
            if (!base.RemoveChild(child))
                return false;

            for (int i = 0; i < _sub_panels.Length; i++)
            {
                if (_sub_panels[i] == child)
                    _sub_panels[i] = null;
            }
            return true;
        }

        public EdUIComp Panel1
        {
            get { return _sub_panels[0]; }
            set
            {
                if (_sub_panels[0] == value)
                    return;

                RemoveChild(_sub_panels[0]);
                if (value == null)
                    return;

                if (!base.AddChild(value))
                    return;
                _sub_panels[0] = value;
            }
        }
        public EdUIComp Panel2
        {
            get { return _sub_panels[1]; }
            set
            {
                if (_sub_panels[1] == value)
                    return;

                RemoveChild(_sub_panels[1]);
                if (value == null)
                    return;

                if (!base.AddChild(value))
                    return;
                _sub_panels[1] = value;
            }
        }

        public override void Draw(Vector2 size)
        {
            if (!Visible)
                return;

            Rect rect = new Rect();

            if (Dir == E_DIR.Horizontal)
            {
                Vector2 size1 = new Vector2(size.x * _split_pos, size.y);
                Vector2 size2 = new Vector2(size.x * (1 - _split_pos), size.y);
                rect.size = size1;
                GUI.BeginGroup(rect);
                Panel1?.Draw(size1);
                GUI.EndGroup();

                rect.size = size2;
                rect.x = size1.x;
                GUI.BeginGroup(rect);
                Panel2?.Draw(size2);
                GUI.EndGroup();
            }
            else
            {
                Vector2 size1 = new Vector2(size.x, size.y * _split_pos);
                Vector2 size2 = new Vector2(size.x, size.y * (1 - _split_pos));
                rect.size = size1;
                GUI.BeginGroup(rect);
                Panel1?.Draw(size1);
                GUI.EndGroup();

                rect.size = size2;
                rect.y = size1.y;
                GUI.BeginGroup(rect);
                Panel2?.Draw(size2);
                GUI.EndGroup();
            }

            ResizeSplitFirstView(size);
        }
        
        private void ResizeSplitFirstView(Vector2 size)
        {
            Rect handle_rect;
            if (Dir == E_DIR.Horizontal)
            {
                handle_rect = new Rect(size.x * _split_pos - 1, 0, 2f, size.y);
                GUI.DrawTexture(handle_rect, Texture2D.whiteTexture);
                EditorGUIUtility.AddCursorRect(handle_rect, MouseCursor.ResizeHorizontal);
            }
            else
            {
                handle_rect = new Rect(0, size.y * _split_pos - 1, size.x, 2f);
                GUI.DrawTexture(handle_rect, Texture2D.whiteTexture);
                EditorGUIUtility.AddCursorRect(handle_rect, MouseCursor.ResizeVertical);
            }

            if (Event.current.type == EventType.MouseDown && handle_rect.Contains(Event.current.mousePosition))
            {
                _resize_flag = true;
            }

            if (_resize_flag)
            {
                if (Dir == E_DIR.Horizontal)
                    SplitPos = Event.current.mousePosition.x / size.x;
                else
                    SplitPos = Event.current.mousePosition.y / size.y;
            }
            if (Event.current.type == EventType.MouseUp)
                _resize_flag = false;
        }
    }
}
