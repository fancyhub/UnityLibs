/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/22
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif


namespace FH.UI
{
    /// <summary>
    /// Ref: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
    /// 
    /// Example: 
    /// <quad img=sprite_name size=40 width=2.0 height=1.0/>
    /// <quad img=sprite_name size=40 width=2.0/>
    /// <quad img=sprite_name size=40/>
    /// 
    /// width: default 1
    /// height: default 1
    /// Sprite Size: (size*width/height) X (size)
    /// </summary>
    public class UIRichText : UnityEngine.UI.Text
    {
        private UIRichTextHelper _Helper = new UIRichTextHelper();
        public override string text
        {
            set
            {
                base.text = value;
                _Helper.ParseText(text);
                _Helper.SyncImagesCount(transform);
            }
        }

        protected override void Start()
        {
            base.Start();
            _Helper.ParseText(text);
            _Helper.SyncImagesCount(transform);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            m_DisableFontTextureRebuiltCallback = true;
            base.OnPopulateMesh(toFill);
            m_DisableFontTextureRebuiltCallback = false;

            _Helper.CalcImageQuadRect(toFill);
            _Helper.SyncImagePos();
        }

#if UNITY_EDITOR
        public void EdDirty()
        {
            _Helper.ParseText(text);
            _Helper.SyncImagesCount(transform);
        }
#endif
    }


    internal sealed class UIRichTextHelper
    {
        private const int CVertCountPerChar = 4;
        private const string CQuadPlaceHolder = "&&&&";

        private static readonly Regex S_TagQuadReg = new Regex(@"<quad(?<content>[\s\w=\.]*)/?>", RegexOptions.IgnoreCase);
        private static readonly Regex S_TagReg = new Regex(@"</?.*/?>", RegexOptions.IgnoreCase);

        //所有的 attr 后面不要有空格
        private const string CAttrImageName = "img="; //string
        private const string CAttrSize = "size=";  //int
        private const string CAttrWidth = "width="; //float
        private const string CAttrHeight = "height="; //float

        private static readonly UIVertex[] S_TempVert = new UIVertex[CVertCountPerChar];

        private string _Text;

        private struct ImageInfo
        {
            public int CharIndex;
            public string ImageName;
            public Rect QuadRect;

            public int Size;
            public float Width;
            public float Height;

            public ImageInfo(int char_index)
            {
                this.CharIndex = char_index;
                this.ImageName = null;
                this.QuadRect = new Rect();
                this.Size = 0;
                this.Width = 1;
                this.Height = 1;
            }

            public Vector2 CalcSize()
            {
                if (Height > 0.0001f)
                {
                    return new Vector2(Size * Width / Height, Size);
                }

                return new Vector2(Size, Size);
            }
        }

        private List<ImageInfo> _InfoList = new List<ImageInfo>();
        private List<Image> _ImageList;

        public void ParseText(string text)
        {
            //1. 检查是否发生了变化
            if (_Text == text)
                return;
            _Text = text;

            _InfoList.Clear();

            //2. 计算 每个 <quad> 在整个渲染的text quad vert 第几个
            {
                string vertex_text = _Convert2VertexData(text);
                int start_offset = 0;
                for (; ; )
                {
                    start_offset = vertex_text.IndexOf(CQuadPlaceHolder, start_offset);
                    if (start_offset < 0)
                        break;

                    int char_index = start_offset - _InfoList.Count * (CQuadPlaceHolder.Length - 1);
                    start_offset += CQuadPlaceHolder.Length;
                    _InfoList.Add(new ImageInfo(char_index));
                }

                if (_InfoList.Count == 0)
                    return;
            }

            //3. 分析原始text 里面的 <quad> 的属性 name=xx size=xx width=xx height=xx
            {
                MatchCollection tagQuadMatchCollection = S_TagQuadReg.Matches(text);
                if (tagQuadMatchCollection.Count != _InfoList.Count)
                {
                    Debug.LogErrorFormat("解析的数量不一致 {0}:{1}", _InfoList.Count, tagQuadMatchCollection.Count);
                    _InfoList.Clear();
                    return;
                }

                for (int i = 0; i < tagQuadMatchCollection.Count; i++)
                {
                    ImageInfo info = _InfoList[i];
                    Match match = tagQuadMatchCollection[i];
                    ReadOnlySpan<char> content = match.Groups["content"].Value;

                    //attr:  name=xxx
                    int attr_start_index = content.IndexOf(CAttrImageName);
                    if (attr_start_index >= 0)
                    {
                        ReadOnlySpan<char> attr_name = content.Slice(attr_start_index + CAttrImageName.Length);
                        int attr_end_index = attr_name.IndexOf(' ');
                        if (attr_end_index >= 0)
                        {
                            attr_name = attr_name.Slice(0, attr_end_index);
                        }

                        if (attr_name.Length > 0)
                            info.ImageName = attr_name.ToString();
                        //Debug.Log($"{content.ToString()}, AttrName: \"{attr_name.ToString()}\"");
                    }

                    //attr:  size=xxx
                    attr_start_index = content.IndexOf(CAttrSize);
                    if (attr_start_index >= 0)
                    {
                        ReadOnlySpan<char> attr_size = content.Slice(attr_start_index + CAttrSize.Length);
                        int attr_end_index = attr_size.IndexOf(' ');
                        if (attr_end_index >= 0)
                        {
                            attr_size = attr_size.Slice(0, attr_end_index);
                        }
                        //Debug.Log($"{content.ToString()}, AttrSize: \"{attr_size.ToString()}\"");
                        int.TryParse(attr_size, out info.Size);
                    }

                    //attr:  width=xxx
                    attr_start_index = content.IndexOf(CAttrWidth);
                    if (attr_start_index >= 0)
                    {
                        ReadOnlySpan<char> attr_width = content.Slice(attr_start_index + CAttrWidth.Length);
                        int attr_end_index = attr_width.IndexOf(' ');
                        if (attr_end_index >= 0)
                        {
                            attr_width = attr_width.Slice(0, attr_end_index);
                        }
                        //Debug.Log($"{content.ToString()}, AttrWidth: \"{attr_width.ToString()}\"");
                        if (float.TryParse(attr_width, out float width))
                        {
                            info.Width = width;
                        }
                    }

                    //attr:  height=xxx
                    attr_start_index = content.IndexOf(CAttrHeight);
                    if (attr_start_index >= 0)
                    {
                        ReadOnlySpan<char> attr_height = content.Slice(attr_start_index + CAttrHeight.Length);
                        int attr_end_index = attr_height.IndexOf(' ');
                        if (attr_end_index > 0)
                        {
                            attr_height = attr_height.Slice(0, attr_end_index);
                        }
                        //Debug.Log($"{content.ToString()}, AttrHeight: {attr_height.ToString()}");
                        if (float.TryParse(attr_height, out float height))
                        {
                            info.Height = height;
                        }
                    }

                    _InfoList[i] = info;
                }
            }
        }

        public void CalcImageQuadRect(VertexHelper toFill)
        {
            int total_count = toFill.currentVertCount;
            for (var i = 0; i < _InfoList.Count; i++)
            {
                var imgInfo = _InfoList[i];

                int start_index = imgInfo.CharIndex * CVertCountPerChar;
                int end_index = start_index + CVertCountPerChar - 1;

                if (start_index < 0 || start_index >= total_count || end_index < 0 || end_index >= total_count)
                {
                    imgInfo.QuadRect = new Rect();
                    _InfoList[i] = imgInfo;
                    continue;
                }

                toFill.PopulateUIVertex(ref S_TempVert[0], start_index);
                toFill.PopulateUIVertex(ref S_TempVert[1], start_index + 1);
                toFill.PopulateUIVertex(ref S_TempVert[2], start_index + 2);
                toFill.PopulateUIVertex(ref S_TempVert[3], start_index + 3);

                //把这4个点退化掉
                for (int j = end_index; j > start_index; j--)
                {
                    toFill.SetUIVertex(S_TempVert[0], j);
                }

                float min_x = Math.Min(Math.Min(S_TempVert[0].position.x, S_TempVert[1].position.x), Math.Min(S_TempVert[2].position.x, S_TempVert[3].position.x));
                float max_x = Math.Max(Math.Max(S_TempVert[0].position.x, S_TempVert[1].position.x), Math.Max(S_TempVert[2].position.x, S_TempVert[3].position.x));
                float min_y = Math.Min(Math.Min(S_TempVert[0].position.y, S_TempVert[1].position.y), Math.Min(S_TempVert[2].position.y, S_TempVert[3].position.y));
                float max_y = Math.Max(Math.Max(S_TempVert[0].position.y, S_TempVert[1].position.y), Math.Max(S_TempVert[2].position.y, S_TempVert[3].position.y));

                imgInfo.QuadRect = new Rect(min_x, min_y, max_x - min_x, max_y - min_y);
                _InfoList[i] = imgInfo;
            }
        }

        public void SyncImagesCount(Transform tran)
        {
            //1. 获取子节点
            if (_ImageList == null)
            {
                _ImageList = new List<Image>();
                for (int i = 0; i < tran.childCount; i++)
                {
                    var child = tran.GetChild(i);
                    var image = child.GetComponent<Image>();
                    if (image == null)
                        continue;
                    _ImageList.Add(image);
                    image.gameObject.SetActive(false);
                }
            }


            //2. 移除失效的
            for (int i = _ImageList.Count - 1; i >= 0; i--)
            {
                if (_ImageList[i] == null)
                {
                    _ImageList.RemoveAt(i);
                }
            }

            //3. 创建不够的
            for (int i = _ImageList.Count; i < _InfoList.Count; i++)
            {
                _ImageList.Add(_CreateImage2(tran));
            }

            //4. 隐藏多余的
            for (int i = _InfoList.Count; i < _ImageList.Count; i++)
            {
                _ImageList[i].gameObject.SetActive(false);
            }

            //5. 加载
            for (int i = 0; i < _InfoList.Count; i++)
            {
                Image image = _ImageList[i];
                ImageInfo info = _InfoList[i];
                if (info.ImageName == null)
                {
                    image.gameObject.SetActive(false);
                    continue;
                }

                image.gameObject.SetActive(true);
                image.rectTransform.sizeDelta = info.CalcSize();
                image.ExtAsyncSetSprite(info.ImageName);
            }
        }

        public void SyncImagePos()
        {
            if (_ImageList == null)
                return;
            for (int i = 0; i < _InfoList.Count; i++)
            {
                ImageInfo info = _InfoList[i];
                if (i >= _ImageList.Count)
                    return;

                Image image = _ImageList[i];
                if (image == null)
                    continue;

                image.rectTransform.anchoredPosition = info.QuadRect.center;

                //移走
                if (info.QuadRect.size.x < 0.001f || info.QuadRect.size.y < 0.001f)
                {
                    //Debug.Log($"Index: {i}, {info.QuadRect},   {info.CalcSize()}");
                    image.rectTransform.anchoredPosition = Vector2.one * 10000;
                }

                //不能在这边调用
                //image.rectTransform.sizeDelta = info.QuadRect.size;
            }
        }


        private static string _Convert2VertexData(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string temp = S_TagQuadReg.Replace(text, CQuadPlaceHolder);
            temp = S_TagReg.Replace(temp, "");
            temp = temp.Replace("\n", "");
            temp = temp.Replace(" ", "");
            //Log.E($"{text} -> {temp}");
            return temp;
        }


        private static Image _CreateImage(Transform tran)
        {
            DefaultControls.Resources resources = new DefaultControls.Resources();
            GameObject go = DefaultControls.CreateImage(resources);
            go.hideFlags = HideFlags.DontSaveInEditor;
            go.layer = tran.gameObject.layer;
            go.transform.SetParent(tran, false);
            return go.GetComponent<Image>();
        }

        private static Image _CreateImage2(Transform tran)
        {
            GameObject go = new GameObject("Image");
            go.layer = tran.gameObject.layer;
            go.hideFlags = HideFlags.DontSaveInEditor;
            go.transform.SetParent(tran, false);
            return go.AddComponent<Image>();
        }
    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, CustomEditor(typeof(UIRichText), false)]
    public class UIRichTextEditor : UnityEditor.UI.GraphicEditor
    {
        SerializedProperty m_Text;
        SerializedProperty m_FontData;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (var p in targets)
                {
                    ((UIRichText)p).EdDirty();
                }
            }
        }
    }
#endif
}