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
        public float MoveDistance;
    }

    public static class NoticeEffectPlayer
    {
        public static void Play(RectTransform view, float progress, List<NoticeEffectItemConfig> effects)
        {
            foreach (var e in effects)
            {
                Play(view, progress, e);
            }
        }

        public static bool Play(RectTransform view, float progress, NoticeEffectItemConfig effect)
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

                case ENoticeEffect.FadeIn:
                    {
                        float alpha = progress;
                        _SetViewAlpha(view, alpha);
                    }
                    break;

                case ENoticeEffect.FadeOut:
                    {
                        float alpha = progress;
                        _SetViewAlpha(view, 1 - alpha);
                    }
                    break;

                case ENoticeEffect.MoveUp:
                    {
                        float dist = effect.MoveDistance * progress;
                        _SetViewPosY(view, dist);
                    }
                    break;

                case ENoticeEffect.RightSlideIn:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = -size.x * 0.5f;
                        float pos_x_hide = size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_hide, pos_x_show, progress);
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.RightSlideOut:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = -size.x * 0.5f;
                        float pos_x_hide = size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_show, pos_x_hide, progress);
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.LeftSlideIn:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = size.x * 0.5f;
                        float pos_x_hide = -size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_hide, pos_x_show, progress);
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.LeftSlideOut:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = size.x * 0.5f;
                        float pos_x_hide = -size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_show, pos_x_hide, progress);
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.ScaleIn:
                    {
                        Vector3 show = Vector3.one;
                        Vector3 hide = new Vector3(1, 0, 1);
                        Vector3 scale = Vector3.Lerp(hide, show, progress);
                        view.localScale = scale;
                    }
                    break;


                case ENoticeEffect.ScaleOut:
                    {
                        Vector3 show = Vector3.one;
                        Vector3 hide = new Vector3(1, 0, 1);
                        Vector3 scale = Vector3.Lerp(show, hide, progress);
                        view.localScale = scale;
                    }
                    break;
            }
            return true;
        }


        private static void _SetViewAlpha(RectTransform view, float alpha)
        {
            if (view == null)
                return;
            CanvasGroup group = view.GetComponent<CanvasGroup>();
            if (group == null)
                group = view.gameObject.AddComponent<CanvasGroup>();
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
