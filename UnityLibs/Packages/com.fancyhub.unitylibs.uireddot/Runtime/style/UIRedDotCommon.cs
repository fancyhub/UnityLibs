/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/17 11:32:13
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public class UIRedDotCommon : UIRedDotBehaviour
    {
        public enum EWatchType
        {
            Count,
            IncrementFlag,
        }
        public EWatchType _watch_type = EWatchType.Count;
        public GameObject _obj;
        public UnityEngine.UI.Text _text;

        protected override void OnEvent(Str key, UIRedDotValue val)
        {            
            if (_obj != null)
            {
                switch (_watch_type)
                {
                    case EWatchType.Count:
                        _obj.ExtSetGameObjectActive(val.Count > 0);
                        break;

                    case EWatchType.IncrementFlag:
                        _obj.ExtSetGameObjectActive(val.IncrementFlag);
                        break;

                    default:
                        _obj.ExtSetGameObjectActive(false);
                        break;
                }
            }

            if (_text != null)
                _text.text = val.Count.ToString();
        }
    }
}
