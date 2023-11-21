/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/8/8
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH.UI
{
    public interface IUIResHolder
    {
        public GameObject Create(string res_path, Transform parent);
        public void Release(GameObject obj);
        public void Destroy();
    }

    public abstract partial class UIBaseView
    {
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

        private IUIResHolder _res_holder;
        private GameObject _self_root;
        private UICanvasOrder _canvas_order;
        private EUIBaseViewCreateMode _view_create_mode;
        private EUIBaseViewLifeState _view_life_state;

        public IUIResHolder ResHolder => _res_holder;
        public GameObject SelfRoot => _self_root;
        public UICanvasOrder CanvasOrder => _canvas_order;


        public virtual string GetAssetPath() { return null; }
        public virtual string GetResoucePath() { return null; }
        public virtual string GetDebugName() { return this.GetType().Name; }

        #region 需要自己实现的, 可选项
        public virtual void OnCreate() { }
        public virtual void OnDestroy() { }
        #endregion

        #region 不要修改
        protected static T CreateSub<T>(GameObject obj_self, IUIResHolder res_holder) where T : UIBaseView, new()
        {
            T ret = new T();
            if (ret.Init(obj_self, res_holder, EUIBaseViewCreateMode.Sub))
                return ret;
            return null;
        }

        public bool Init(GameObject obj_self, IUIResHolder res_holder, EUIBaseViewCreateMode create_mode)
        {
            //1. Check
            if (_view_life_state != EUIBaseViewLifeState.None)
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

        protected UIViewReference _FindViewReference(string prefab_name)
        {
            UIViewReference ret = null;
            var comps = _self_root.ExtGetComps<UIViewReference>();
            for (int i = 0; i < comps.Count; i++)
            {
                if (comps[i]._prefab_name == prefab_name)
                {
                    ret = comps[i];
                    break;
                }
            }
            comps.Clear();
            return ret;
        }

        public void Destroy()
        {
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


        protected virtual void _AutoInit() { }
        protected virtual void _AutoDestroy() { }


        #endregion
    }
}
