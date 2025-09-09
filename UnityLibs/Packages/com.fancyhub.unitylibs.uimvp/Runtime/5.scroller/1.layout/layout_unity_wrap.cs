/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    public abstract class ScrollLayoutUnityWrap : IScrollLayout
    {
        public abstract void Build(IScroll scroller);

        public abstract bool UpdateParams();

        public virtual bool EdChanged()
        {
            return UpdateParams();
        }

        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange;

        public static int GetCount(ScrollRect scroll_rect, LayoutGroup group)
        {
            if (scroll_rect == null)
                return 1;

            switch (group)
            {
                case HorizontalLayoutGroup h:
                    return 1;

                case VerticalLayoutGroup v:
                    return 1;

                case GridLayoutGroup g:
                    if (g.constraint != GridLayoutGroup.Constraint.Flexible)
                        return g.constraintCount;
                    return 1;
                default:
                    return 1;
            }
        }

        public static void GetPadding(LayoutGroup group, out int left, out int right, out int top, out int bottom)
        {
            if (group == null)
            {
                left = 0;
                right = 0;
                top = 0;
                bottom = 0;
                return;
            }
            ScrollLayoutPadding padding = ScrollLayoutPadding.From(group.padding);
            left = padding.Left;
            right = padding.Right;
            top = padding.Top;
            bottom = padding.Bottom;
        }

        public static float GetAlignment(ScrollRect scroll_rect, LayoutGroup group)
        {
            if (scroll_rect == null)
                return 0;

            Vector2 alignment = Vector2.zero;
            switch (group.childAlignment)
            {
                case TextAnchor.UpperLeft:
                    alignment = new Vector2(0, 0);
                    break;

                case TextAnchor.UpperCenter:
                    alignment = new Vector2(0.5f, 0);
                    break;

                case TextAnchor.UpperRight:
                    alignment = new Vector2(1, 0);
                    break;

                case TextAnchor.MiddleLeft:
                    alignment = new Vector2(0, 0.5f);
                    break;

                case TextAnchor.MiddleCenter:
                    alignment = new Vector2(0.5f, 0.5f);
                    break;

                case TextAnchor.MiddleRight:
                    alignment = new Vector2(1, 0.5f);
                    break;

                case TextAnchor.LowerLeft:
                    alignment = new Vector2(0, 1);
                    break;

                case TextAnchor.LowerCenter:
                    alignment = new Vector2(0.5f, 1);
                    break;

                case TextAnchor.LowerRight:
                    alignment = new Vector2(1, 1);
                    break;

                default:
                    alignment = Vector2.zero;
                    break;
            }

            if (scroll_rect.vertical)
                return alignment.x;
            return alignment.y;
        }

        public static float GetSpacing(ScrollRect scroll_rect, LayoutGroup group)
        {
            if (scroll_rect == null)
                return 0;
            if (scroll_rect.vertical)
            {
                switch (group)
                {
                    case HorizontalLayoutGroup h:
                        return 0;

                    case VerticalLayoutGroup v:
                        return v.spacing;

                    case GridLayoutGroup g:
                        return g.spacing.y;
                    default:
                        return 0;
                }
            }
            else
            {
                switch (group)
                {
                    case HorizontalLayoutGroup h:
                        return h.spacing;

                    case VerticalLayoutGroup v:
                        return 0;

                    case GridLayoutGroup g:
                        return g.spacing.x;
                    default:
                        return 0;
                }
            }
        }
    }

    //水平排布
    public class ScrollLayoutUnityWrapH : ScrollLayoutUnityWrap
    {
        private LayoutGroup _unity_layout;
        private ScrollRect _unity_scroll;
        private ScrollLayoutHCol _layout;

        public ScrollLayoutUnityWrapH(ScrollRect scroll_rect, LayoutGroup unity_layout)
        {
            _unity_layout = unity_layout;
            _unity_scroll = scroll_rect;
            _layout = new ScrollLayoutHCol();
        }

        public override bool UpdateParams()
        {
            ScrollLayoutPadding padding = ScrollLayoutPadding.From(_unity_layout.padding);
            float spacing = GetSpacing(_unity_scroll, _unity_layout);
            float alignment = GetAlignment(_unity_scroll, _unity_layout);
            int count = GetCount(_unity_scroll, _unity_layout);

            bool changed = false;
            changed = changed || (Mathf.Abs(_layout.Spacing - spacing) > float.Epsilon);
            changed = changed || (Mathf.Abs(_layout.Alignment - alignment) > float.Epsilon);
            changed = changed || (!padding.IsEqual(_layout.Padding));
            changed = changed || (_layout.Count != count);

            _layout.Padding = padding;
            _layout.Spacing = spacing;
            _layout.Alignment = alignment;
            _layout.Count = count;
            return changed;
        }

        public override void Build(IScroll scroller)
        {
            _layout.Build(scroller);
        }
    }

    //竖直
    public class ScrollLayoutUnityWrapV : ScrollLayoutUnityWrap
    {
        private LayoutGroup _unity_layout;
        private ScrollRect _unity_scroll;
        private ScrollScrollLayoutVRow _layout;
        public ScrollLayoutUnityWrapV(ScrollRect scroll_rect, LayoutGroup unity_layout)
        {
            _unity_layout = unity_layout;
            _unity_scroll = scroll_rect;
            _layout = new ScrollScrollLayoutVRow();
        }

        public override bool UpdateParams()
        {
            ScrollLayoutPadding padding = ScrollLayoutPadding.From(_unity_layout.padding);
            float spacing = GetSpacing(_unity_scroll, _unity_layout);
            float alignment = GetAlignment(_unity_scroll, _unity_layout);
            int count = GetCount(_unity_scroll, _unity_layout);

            bool changed = false;
            changed = changed || (Mathf.Abs(_layout.Spacing - spacing) > float.Epsilon);
            changed = changed || (Mathf.Abs(_layout.Alignment - alignment) > float.Epsilon);
            changed = changed || (!padding.IsEqual(_layout.Padding));
            changed = changed || (_layout.Count != count);

            _layout.Padding = padding;
            _layout.Spacing = spacing;
            _layout.Alignment = alignment;
            _layout.Count = count;
            return changed;
        }

       

        public override void Build(IScroll scroller)
        {
            _layout.Build(scroller);
        }
    }


    public static class ScrollLayoutUnityWrapFactory
    {
        public static IScrollLayout Create(ScrollRect scroll_rect, Transform content = null)
        {
            //1. 获取 Unity 的Layout
            if (content == null)
                content = scroll_rect.content;
            LayoutGroup unity_layout = content.GetComponent<LayoutGroup>();

            //2. 如果没有layout,直接创建 简单的Layout
            if (unity_layout == null)
            {
                if (scroll_rect.vertical)
                    return new ScrollLayoutV();
                return new ScrollLayoutH();
            }
            unity_layout.enabled = false;

            //3. 创建Unity Wrap 的Layout
            ScrollLayoutUnityWrap unity_wrap = null;
            if (scroll_rect.vertical)
            {
                unity_wrap = new ScrollLayoutUnityWrapV(scroll_rect, unity_layout);
            }
            else
                unity_wrap = new ScrollLayoutUnityWrapH(scroll_rect, unity_layout);
            return unity_wrap;
        }
    }
}
