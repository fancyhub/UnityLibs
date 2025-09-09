/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FH.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIScrollMovement : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IScrollMovement
    {
        public const float C_THRESHOLD = 1.0f;

        public IScrollEvent ScrollerEvent { get; set; }

        public float _threshold = C_THRESHOLD;

        public enum EStatus
        {
            Stop,
            Scrolling,
            Moving,
        }

        public EStatus _status = EStatus.Stop;
        private ScrollRect _scroll_rect;

        public static UIScrollMovement Get(ScrollRect rect)
        {
            return rect.ExtGetComp<UIScrollMovement>(true);
        }

        public float Threshold { get { return _threshold; } set { _threshold = Math.Max(float.Epsilon, value); } }

        private void Awake()
        {
            _status = EStatus.Stop;
            _scroll_rect = GetComponent<ScrollRect>();
            _scroll_rect.onValueChanged.AddListener(_on_scroll_move);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!_scroll_rect.IsActive())
                return;
            _status = EStatus.Scrolling;
            ScrollerEvent?.OnScrollDragStart();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _status = EStatus.Moving;
            ScrollerEvent?.OnScrollDragEnd();
        }

        public void StopMovement()
        {
            _scroll_rect.StopMovement();
        }

        private void OnDisable()
        {
            _status = EStatus.Stop;
        }

        private void Update()
        {
            _UpdateMovement();
            ScrollerEvent?.OnScrollUpdate();
        }

        private void _UpdateMovement()
        {
            if (_status != EStatus.Moving)
                return;


            if (!_is_velocity_zero(_scroll_rect.velocity, _threshold))
                return;
            _scroll_rect.StopMovement();
            _status = EStatus.Stop;
            ScrollerEvent?.OnScrollMoveEnd();
        }

        private void _on_scroll_move(Vector2 pos)
        {
            ScrollerEvent?.OnScrollMoving();
        }

        private static bool _is_velocity_zero(Vector2 velocity, float threshold)
        {
            if (Math.Abs(velocity.x) < threshold && Mathf.Abs(velocity.y) < threshold)
                return true;
            return false;
        }
    }
}
