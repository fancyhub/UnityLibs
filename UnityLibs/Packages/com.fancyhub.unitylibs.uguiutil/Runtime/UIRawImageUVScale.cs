using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FH.UI
{
    [RequireComponent(typeof(RawImage))]
    public class UIRawImageUVScale : UIBehaviour
    {
        private RawImage _RawImg;
        public ScaleMode ScaleMode = ScaleMode.ScaleAndCrop;
        protected override void Awake()
        {
            _RawImg = GetComponent<RawImage>();
        }

        protected override void OnEnable()
        {
            _RawImg.ExtScaleUV(ScaleMode);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            _RawImg.ExtScaleUV(ScaleMode);
        }
    }
}
