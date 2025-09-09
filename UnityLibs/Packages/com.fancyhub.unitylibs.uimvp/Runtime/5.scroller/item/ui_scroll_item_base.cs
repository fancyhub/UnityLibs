/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30 18:18:00
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public abstract class ScrollItemBase : IScrollerItem
    {
        public IScrollerItemParent _parent;
        public Vector2 _pos;
        public Vector2 _anim_pos = Vector2.zero;

        public virtual void SetParent(IScrollerItemParent parent)
        {
            _parent = parent;
        }

        public Vector2 Pos
        {
            get { return _pos; }
            set
            {

                if (_pos.Equals(value))
                    return;
                _pos = value;
                OnPosChange();
            }
        }

        public abstract Vector2 Size { get; }

        public Vector2 AnimPos
        {
            get { return _anim_pos; }
            set
            {
                if (_anim_pos.Equals(value))
                    return;
                _anim_pos = value;
                OnPosChange();
            }
        }

        public Vector2 FinalPos
        {
            get { return _anim_pos + _pos; }
        }


        public static Vector3 CalcLocalPos(
            Vector2 pos
            , Vector2 size
            , Vector2 pivot)
        {
            float x = pos.x + size.x * pivot.x;
            float y = -pos.y - size.y * pivot.y;

            return new Vector3(x, y);
        }

        public abstract void Destroy();
        public abstract bool CullVisible { get; set; }
        protected abstract void OnPosChange();
    }
}
