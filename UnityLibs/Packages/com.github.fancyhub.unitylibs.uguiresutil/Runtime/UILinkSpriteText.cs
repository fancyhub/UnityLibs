/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;


namespace FH.UI
{
    /// <summary>
    /// 文本控件，支持图片
    /// </summary>    
    public class UILinkSpriteText : Text
    {
        [TextArea(3, 10), SerializeField]
        protected string originText;

        public override string text
        {
            get => _outputText;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        return;
                    }

                    originText = String.Empty;
                    _mTextDirty = true;
                    SetVerticesDirty();
                }
                else
                {
                    if (originText == value)
                    {
                        return;
                    }

                    originText = value;
                    _mTextDirty = true;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// 解析完最终的文本
        /// </summary>
        private string _outputText;


        /// <summary>
        /// 对应的顶点输出的文本
        /// </summary>
        private string _vertexText;

        /// <summary>
        /// 是否需要解析
        /// </summary>
        private bool _mTextDirty = true;


        /// <summary>
        /// 图片池
        /// </summary>
        private readonly List<Image> _mImagesPool = new List<Image>();

        /// <summary>
        /// 图片的最后一个顶点的索引
        /// </summary>
        private readonly List<int> _mImagesVertexIndex = new List<int>();

        /// <summary>
        /// 正则取出所需要的属性
        /// </summary>
        private static readonly Regex ImageRegex = new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            _mTextDirty = true;
            UpdateQuadImage();
        }


        private void UpdateQuadImage()
        {
            if (_mTextDirty)
            {
                _outputText = GetOutputText(originText);
            }

            _mImagesVertexIndex.Clear();
            int startSearchIndex = 0;
            var matches = ImageRegex.Matches(originText);
            for (var i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int index = _vertexText.IndexOf('&', startSearchIndex);

                var firstIndex = index * 4;
                startSearchIndex = index + 1;

                _mImagesVertexIndex.Add(firstIndex);

                _mImagesPool.RemoveAll(image => image == null);
                if (_mImagesPool.Count == 0)
                {
                    GetComponentsInChildren(_mImagesPool);
                }

                if (_mImagesVertexIndex.Count > _mImagesPool.Count)
                {
                    var resources = new DefaultControls.Resources();
                    var go = DefaultControls.CreateImage(resources);
                    go.layer = gameObject.layer;
                    var rt = go.transform as RectTransform;
                    if (rt)
                    {
                        rt.SetParent(rectTransform, false);
                        rt.localPosition = Vector3.zero;
                        rt.localRotation = Quaternion.identity;
                        rt.localScale = Vector3.one;
                    }

                    _mImagesPool.Add(go.GetComponent<Image>());
                }

                var spriteName = match.Groups[1].Value;
                var img = _mImagesPool[i];
                if (img.sprite == null || img.sprite.name != spriteName)
                {
                    img.ExtSetSprite(spriteName, true);
                }

                var imgRectTransform = img.GetComponent<RectTransform>();
                if (Int32.TryParse(match.Groups[2].Value, out int size))
                {
                    imgRectTransform.sizeDelta = new Vector2(size, size);
                }
                else
                {
                    Debug.LogWarning("无法正常解析大小");
                    imgRectTransform.sizeDelta = new Vector2(16f, 16f);
                }

                img.enabled = true;
            }

            for (var i = _mImagesVertexIndex.Count; i < _mImagesPool.Count; i++)
            {
                if (_mImagesPool[i])
                {
                    _mImagesPool[i].enabled = false;
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            m_DisableFontTextureRebuiltCallback = true;
            base.OnPopulateMesh(toFill);
            UIVertex vert = new UIVertex();

            for (var i = 0; i < _mImagesVertexIndex.Count; i++)
            {
                var index = _mImagesVertexIndex[i];
                var rt = _mImagesPool[i].rectTransform;
                var size = rt.sizeDelta;
                if (index < toFill.currentVertCount)
                {
                    toFill.PopulateUIVertex(ref vert, index);
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y - size.y * 0.625f);
                    toFill.PopulateUIVertex(ref vert, index);
                    for (int j = index + 3, m = index; j > m; j--)
                    {
                        toFill.SetUIVertex(vert, j);
                    }
                }
            }
            m_DisableFontTextureRebuiltCallback = false;
        }


        protected virtual string GetOutputText(string outputText)
        {
            if (string.IsNullOrEmpty(outputText))
                return "";
            string tempOutputText = outputText;
            _vertexText = outputText;
            _vertexText = Regex.Replace(_vertexText, "<color.*?>", "");
            _vertexText = Regex.Replace(_vertexText, "</color>", "");
            _vertexText = ImageRegex.Replace(_vertexText, "&");
            _vertexText = _vertexText.Replace("\n", "");
            _vertexText = _vertexText.Replace(" ", "");
            _mTextDirty = false;
            return tempOutputText;
        }

    }
}