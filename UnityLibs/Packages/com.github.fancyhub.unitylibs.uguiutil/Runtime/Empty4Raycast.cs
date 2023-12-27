/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace FH.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Empty4Raycast : MaskableGraphic, ICanvasRaycastFilter
    {
        public List<RectTransform> raycast_ignor_list = new List<RectTransform>();
        protected Empty4Raycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }

        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!isActiveAndEnabled)
                return true;
            bool ret = true;
            foreach (RectTransform rct in raycast_ignor_list)
            {
                if (rct.gameObject.activeSelf && rct.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rct, sp, eventCamera))
                {
                    ret = false;
                }
            }
            return ret;
        }
    }
}