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
        public static void Play(RectTransform view, NoticeItemTime time, List<NoticeEffectItemConfig> effects)
        {
            foreach (var e in effects)
            {
                Play(view, time, e);
            }
        }

        public static bool Play(RectTransform view, NoticeItemTime time, NoticeEffectItemConfig effect)
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
                        float alpha = time.GetCurPhaseProgress();
                        _SetViewAlpha(view, alpha);
                    }
                    break;

                case ENoticeEffect.FadeOut:
                    {
                        float alpha = time.GetCurPhaseProgress();
                        _SetViewAlpha(view, 1 - alpha);
                    }
                    break;

                case ENoticeEffect.MoveUp:
                    {
                        float dist = effect.MoveDistance * time.GetCurPhaseProgress();
                        _SetViewPosY(view, dist);
                    }
                    break;

                case ENoticeEffect.SlideFromRight:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = -size.x * 0.5f;
                        float pos_x_hide = size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_hide, pos_x_show, time.GetCurPhaseProgress());
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
                    }
                    break;

                case ENoticeEffect.SlideToRight:
                    {
                        Vector2 size = view.rect.size;
                        float pos_x_show = -size.x * 0.5f;
                        float pos_x_hide = size.x * 0.5f;
                        float pos_x = Mathf.Lerp(pos_x_show, pos_x_hide, time.GetCurPhaseProgress());
                        Vector2 pos = new Vector2(pos_x, 0);
                        view.anchoredPosition = pos;
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
            if(group==null)            
                group=view.gameObject.AddComponent<CanvasGroup>();
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
