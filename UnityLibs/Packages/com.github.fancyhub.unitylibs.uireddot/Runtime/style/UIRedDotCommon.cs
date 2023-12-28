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
        public GameObject _obj;
        public UnityEngine.UI.Text _text;

        protected override void OnEvent(Str key, int val)
        {
            if (_obj != null && _obj != this.gameObject)
                _obj.SetActive(val > 0);

            if (_text != null)
                _text.text = val.ToString();
        }
    }
}
