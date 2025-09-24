/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/8/8
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    /*
    //UIBaseView 必须要实现的接口
    public abstract partial class UIBaseView
    {
        #region Need Implement
        protected T _CreateSub<T>(GameObject obj_self) where T : UIBaseView, new() { return null; }
        protected UIViewReference _FindViewReference(string prefab_name){return null;}
        public void Destroy() { }
        #endregion


        #region Don't Need Impeplemnt
        public virtual string GetPath() { return null; }
        public virtual string GetDebugName() { return this.GetType().Name; }
        protected virtual void _AutoInit() { }
        protected virtual void _AutoDestroy() { }
        #endregion
    }
    //*/

    ///*
    public abstract partial class UIBaseView : UIElement, IUIView, IUILayerView
    {
        public interface IUIResHolder
        {
            public GameObject Create(string res_path, Transform parent);
            public void Release(GameObject obj);
            public void Destroy();
        }

        public enum EUIBaseViewCreateMode
        {
            RootWithHolder,
            RootWithoutHolder,
            Sub,
        }

        public enum EUIBaseViewLifeState
        {
            None,
            Inited,
            Destroyed,
        }

        public Action EventDestroy;

        private IResHolder _res_holder;
        private GameObject _self_root;
        private RectTransform _self_rect_tran;
        private UICanvasOrder _canvas_order;
        protected EUIBaseViewCreateMode _view_create_mode;
        protected EUIBaseViewLifeState _view_life_state;

        public IResHolder ResHolder => _res_holder;
        public GameObject SelfRoot => _self_root;
        public RectTransform SelfRootTran => _self_rect_tran;
        public UICanvasOrder CanvasOrder => _canvas_order;


        public GameObject GetRoot()
        {
            return _self_root;
        }

        public void SetOrder(int order)
        {
            if (_canvas_order == null)
                return;
            _canvas_order.SetOrder(order, false);
        }

        public void SetOrder(int order, bool relative)
        {
            if (_canvas_order == null)
                return;
            _canvas_order.SetOrder(order, relative);
        }

        public bool Active
        {
            get
            {
                if (_self_root == null)
                    return false;
                return _self_root.activeSelf;
            }

            set
            {
                if (_self_root == null)
                    return;
                if (_self_root.activeSelf != value)
                    _self_root.SetActive(value);
            }
        }

        public virtual Vector2 GetViewSize()
        {
            if (_self_rect_tran == null)
                return Vector2.one;
            return _self_rect_tran.rect.size;
        }

        public virtual Vector2 GetPivot()
        {
            if (_self_rect_tran == null)
                return Vector2.one;
            return _self_rect_tran.pivot;
        }


        public virtual void SetLocalPos(Vector3 local_pos)
        {
            if (null == _self_rect_tran)
                return;
            _self_rect_tran.localPosition = local_pos;
        }

        public Vector3 GetLocalPos()
        {
            if (null == _self_rect_tran)
                return Vector3.zero;
            return _self_rect_tran.localPosition;
        }

        #region Don't Need Impeplemnt
        public virtual string GetPath() { return null; }
        public virtual string GetDebugName() { return this.GetType().Name; }
        protected virtual void _AutoInit() { }
        protected virtual void _AutoDestroy() { }
        #endregion


        #region Need Implement

        public virtual void OnCreate() { }
        public virtual void OnDestroy() { }

        protected T _CreateSub<T>(GameObject obj_self) where T : UIBaseView, new()
        {
            if (obj_self == null)
                return null;

            // T ret = UIViewCache.Get<T>(obj_self);
            // if (ret == null)
            // {
            //     ret = new T();
            //     UIViewCache.Add(obj_self, ret);
            // }
            T ret = new T();
            if (ret._Init(obj_self, _res_holder, EUIBaseViewCreateMode.Sub))
                return ret;
            return null;
        }
        protected UIViewCompReference _FindViewReference(string prefab_name)
        {
            return UIViewCompReference.Find(_self_root, prefab_name);
        }

        private bool _Init(GameObject obj_self, IResHolder res_holder, EUIBaseViewCreateMode create_mode)
        {
            //1. Check
            if (_view_life_state == EUIBaseViewLifeState.Inited)
                return false;
            if (obj_self == null)
                return false;
            if (res_holder == null)
                return false;

            //2. Base Field Init
            if (create_mode == EUIBaseViewCreateMode.RootWithHolder ||
                create_mode == EUIBaseViewCreateMode.RootWithoutHolder)
                this._canvas_order = UICanvasOrder.Get(obj_self);
            _res_holder = res_holder;
            _self_root = obj_self;
            _self_rect_tran = _self_root.transform as RectTransform;
            _view_life_state = EUIBaseViewLifeState.Inited;
            _view_create_mode = create_mode;

            //3. Call 
            _AutoInit();
            OnCreate();

            //4. Set Active
            if (create_mode == EUIBaseViewCreateMode.RootWithHolder ||
                create_mode == EUIBaseViewCreateMode.RootWithoutHolder)
            {
                obj_self.SetActive(true);
            }
            return true;
        }


        public override void Destroy()
        {
            base.Destroy();

            if (_view_life_state == EUIBaseViewLifeState.Destroyed)
            {
                Log.E("destroyed flag is set,Destroy twice {0}", GetDebugName());
                return;
            }
            if (_view_life_state != EUIBaseViewLifeState.Inited)
                return;
            _view_life_state = EUIBaseViewLifeState.Destroyed;

            if (null == _self_root)
            {
                Log.E("Self is Null,Destroy twice {0}", GetDebugName());
                return;
            }

            OnDestroy();
            _AutoDestroy();

            switch (_view_create_mode)
            {
                case EUIBaseViewCreateMode.RootWithHolder:
                    _res_holder.Release(_self_root);
                    _res_holder.Destroy();
                    break;

                case EUIBaseViewCreateMode.RootWithoutHolder:
                    _res_holder.Release(_self_root);
                    break;

                case EUIBaseViewCreateMode.Sub:
                    break;
            }

            _res_holder = null;
            _self_root = null;
            _canvas_order = null;

            var t = EventDestroy;
            EventDestroy = null;
            t?.Invoke();
        }

        #endregion
    }
    //*/
}
