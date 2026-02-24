/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    [System.Serializable]
    public sealed class NoticeEffectItemConfig
    {
        public ENoticeEffect EffectType;
        public float Offset;
    }

    public static class NoticeEffectPlayer
    {
        public static void Play(RectTransform view, float progress, bool fadeIn, List<NoticeEffectItemConfig> effects)
        {
            foreach (var e in effects)
            {
                Play(view, progress, fadeIn, e);
            }
        }

        public static bool Play(RectTransform view, float progress, bool fadeIn, NoticeEffectItemConfig effect)
        {

            if (effect == null)
                return true;
            if (view == null)
                return true;


            switch (effect.EffectType)
            {
                default:
                    NoticeLog.Assert(false, "未处理的类型 {0}", effect.EffectType);
                    return false;

                case ENoticeEffect.Alpha:
                    {
                        float alpha = progress;
                        if (fadeIn)
                        {
                            alpha = Mathf.Lerp(0, 1, progress);
                        }
                        else
                        {
                            alpha = Mathf.Lerp(1, 0, progress);
                        }
                        _SetViewAlpha(view, alpha);
                    }
                    break;

                case ENoticeEffect.OffsetY:
                    {
                        float dist = effect.Offset;

                        if (fadeIn)
                            dist = Mathf.Lerp(effect.Offset, 0, progress);
                        else
                            dist = Mathf.Lerp(0, effect.Offset, progress);
                        _SetViewPosY(view, dist);
                    }
                    break;

                case ENoticeEffect.RightSlide:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = -size.x * 0.5f;
                        float pos_x_hide = size.x * 0.5f;

                        float pos_x = 0;
                        if (fadeIn)
                            pos_x = Mathf.Lerp(pos_x_hide, pos_x_show, progress);
                        else
                            pos_x = Mathf.Lerp(pos_x_show, pos_x_hide, progress);

                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.LeftSlide:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = size.x * 0.5f;
                        float pos_x_hide = -size.x * 0.5f;

                        float pos_x = 0;
                        if (fadeIn)
                            pos_x = Mathf.Lerp(pos_x_hide, pos_x_show, progress);
                        else
                            pos_x = Mathf.Lerp(pos_x_show, pos_x_hide, progress);

                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.ScaleX:
                    {
                        float scale = 1.0f;
                        if (fadeIn)
                            scale = Mathf.Lerp(0.0f, 1.0f, progress);
                        else
                            scale = Mathf.Lerp(1.0f, 0.0f, progress);

                        var localScale = view.localScale;
                        localScale.x = scale;
                        view.localScale=localScale;
                    }
                    break;

                case ENoticeEffect.ScaleY:
                    {
                        float scale = 1.0f;
                        if (fadeIn)
                            scale = Mathf.Lerp(0.0f, 1.0f, progress);
                        else
                            scale = Mathf.Lerp(1.0f, 0.0f, progress);

                        var localScale = view.localScale;
                        localScale.y = scale;
                        view.localScale = localScale;
                    }
                    break;
            }
            return true;
        }


        public static void Reset(RectTransform view)
        {
            _SetViewAlpha(view, 1);
            _SetViewPosY(view, 0);

            if (view != null)
            {
                view.localScale = Vector3.one;
            }
        }         

        private static void _SetViewAlpha(RectTransform view, float alpha)
        {
            if (view == null)
                return;

            CanvasGroup group = view.GetComponent<CanvasGroup>();
            if (group == null)
            {
                if (alpha > 0.99f)
                    return;

                group = view.gameObject.AddComponent<CanvasGroup>();
            }

            group.alpha = alpha;
        }

        private static void _SetViewPosY(RectTransform view, float pos_y)
        {
            if (view == null)
                return;
            Vector3 pos = view.localPosition;
            pos.y = pos_y;
            view.localPosition = pos;
        }
    }
}
