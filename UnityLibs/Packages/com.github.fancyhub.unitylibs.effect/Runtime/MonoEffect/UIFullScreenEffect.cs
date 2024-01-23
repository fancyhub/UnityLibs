using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace FH
{
    public class UIFullScreenEffect : MonoBehaviour, IDynamicComponent
    {
        //ref: https://docs.unity3d.com/ScriptReference/Canvas-sortingOrder.html
        private const int CCanvasOrderMin = -32768;
        private const int CCanvasOrderMax = 32767;

        public enum ERenderOrder
        {
            PreUI,
            PostUI,
            GUI
        }

        public enum EScaleMode
        {
            /// <summary>
            /// 全屏缩放
            /// </summary>
            ScaleFill,

            /// <summary>
            /// 还是全屏,但是保持宽高比,多出的部分被裁剪
            /// </summary>
            scale_crop,
        }

        public bool _AutoUpdate = false;
        public RawImage _RawImage;
        public Animator _Animator;
        public AnimationClip _Clip;


        [System.NonSerialized]
        private AnimationClip _OverrideClip;

        public EScaleMode _Mode;
        [Tooltip("不支持material")]
        public ERenderOrder _Order = ERenderOrder.GUI;

        private Canvas _Canvas;
        private RectTransform _SelfTran;
        private RectTransform _ImgTran;
        private Material _OrigMaterial;
        private Texture _OrigTexture;
        private AnimatorPlayer _Player;

        public void Awake()
        {
            _SelfTran = transform as RectTransform;
            if (_RawImage != null)
            {
                _ImgTran = _RawImage.transform as RectTransform;
                _OrigMaterial = _RawImage.material;
                _OrigTexture = _RawImage.texture;
            }
            _Canvas = GetComponent<Canvas>();
            _Player = new AnimatorPlayer();
        }


        public void SetAnimClip(AnimationClip clip)
        {
            Log.Assert(clip != null, "clip 为空");
            if (_Animator == null)
                return;
            _OverrideClip = clip;

            if (_OverrideClip != null)
                _Player.SetData(_Animator, _OverrideClip);
            else
                _Player.SetData(_Animator, clip);
        }

        public void SetColor(Color c)
        {
            Log.Assert(_RawImage != null);
            if (_RawImage == null)
                return;
            _RawImage.color = c;
        }

        public void SetMaterial(Material mat)
        {
            Log.Assert(_RawImage != null);
            if (_RawImage == null)
                return;
            _RawImage.material = mat;
        }

        public void OnEnable()
        {
            if (_OverrideClip != null)
                _Player.SetData(_Animator, _OverrideClip);
            else
                _Player.SetData(_Animator, _Clip);
            _Player.SetAutoUpdate(_AutoUpdate);
        }

        public void OnDisable()
        {
            _Player.SetData(null, null);
        }

        public void Pause()
        {
            _Animator.speed = 0;
        }

        public void Resume()
        {
            _Animator.speed = 1;
        }

        /// <summary>
        /// 一旦调用就手动设置
        /// </summary>
        public void SetAnimTime(float time)
        {
            if (_Animator == null)
                return;
            _Player.SetAutoUpdate(false);
            _Player.SetAnimTime(time);
        }

        /// <summary>
        /// 一旦调用就手动设置
        /// </summary>
        public void SetAnimProgress(float percent)
        {
            if (_Animator == null)
                return;
            _Player.SetAutoUpdate(false);
            _Player.SetAnimProgress(percent);
        }

        public void SetTexture(Texture texture)
        {
            Log.Assert(_RawImage != null);
            if (_RawImage == null)
                return;
            _RawImage.texture = texture;
            _UpdateImageSize();
        }

        public void SetMode(EScaleMode mode)
        {
            _Mode = mode;
        }

        public void SetPhase(ERenderOrder value)
        {
            if (_Order == value)
                return;
            _Order = value;
            _UpdateOrder();
        }

        public void Update()
        {
            _UpdateOrder();
            _UpdateImageSize();
        }

        public void OnGUI()
        {
            if (_Order != ERenderOrder.GUI)
                return;

            Log.Assert(_RawImage != null);
            if (_RawImage == null)
                return;

            Texture texture = _RawImage.texture;
            if (texture == null)
                texture = Texture2D.whiteTexture;

            Rect rect = new Rect(-1, -1, Screen.width + 2, Screen.height + 2);
            if (_Mode == EScaleMode.scale_crop)
            {
                rect = _CalcRectForCrop(rect, texture);
            }

            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true, 1, _RawImage.color, 0, 0);
        }

        private void _UpdateOrder()
        {
            switch (_Order)
            {
                case ERenderOrder.PreUI:
                    if (_RawImage == null)
                        return;
                    _RawImage.enabled = true;
                    if (_Canvas != null)
                        _Canvas.sortingOrder = CCanvasOrderMin;
                    break;
                case ERenderOrder.PostUI:
                    if (_RawImage == null)
                        return;
                    _RawImage.enabled = true;
                    if (_Canvas != null)
                        _Canvas.sortingOrder = CCanvasOrderMax;
                    break;
                case ERenderOrder.GUI:
                    _RawImage.enabled = false;
                    break;
                default:
                    Log.Assert(false, "未知类型 {0}", _Order);
                    break;
            }
        }

        private void _UpdateImageSize()
        {
            if (_RawImage == null)
                return;
            if (_RawImage.enabled == false)
                return;

            if (_Mode == EScaleMode.ScaleFill || _RawImage.texture == null)
            {
                _ImgTran.sizeDelta = _SelfTran.rect.size;
                return;
            }

            Rect rect = _CalcRectForCrop(_SelfTran.rect, _RawImage.texture);
            _ImgTran.sizeDelta = rect.size;
        }


        private Rect _CalcRectForCrop(Rect screen_rect, Texture texture)
        {
            if (texture == null)
                return screen_rect;

            float img_w = texture.width;
            float img_h = texture.height;
            Rect ret = screen_rect;
            float scree_aspect = screen_rect.width / screen_rect.height;
            float img_aspect = img_w / img_h;
            if (scree_aspect < img_aspect)
            {
                float scale = screen_rect.height / img_h;
                ret.width = img_w * scale;
                ret.x = (screen_rect.width - ret.width) * 0.5f;
            }
            else
            {
                float scale = screen_rect.width / img_w;
                ret.height = img_h * scale;
                ret.y = (screen_rect.height - ret.height) * 0.5f;
            }
            return ret;
        }

        public void OnDynamicRelease()
        {
            _Player.SetData(null, null);
            if (_RawImage != null)
            {
                _RawImage.material = _OrigMaterial;
                _RawImage.texture = _OrigTexture;
            }
        }
    }


    public sealed class AnimatorPlayer
    {
        private Animator _Animator;
        private AnimationClip _Clip;
        private PlayableGraph _Graph;
        private AnimationClipPlayable _ClipPlayable;
        private AnimationPlayableOutput _GraphOut;
        private double _ClipLen;
        private bool _AutoUpdate = false;

        public void SetAutoUpdate(bool enable_auto)
        {
            if (_AutoUpdate == enable_auto)
                return;
            _AutoUpdate = enable_auto;

            if (!_ClipPlayable.IsValid())
                return;

            if (enable_auto)
                _ClipPlayable.SetSpeed(1);
            else
                _ClipPlayable.SetSpeed(0);
        }

        public void SetData(Animator animator, AnimationClip clip)
        {
            //1. 检查是否相等
            if (_Animator == animator && _Clip == clip)
                return;
            _Animator = animator;
            _Clip = clip;
            if (_Graph.IsValid())
                _Graph.Destroy();

            //2. 检查是否为空
            if (_Animator == null || _Clip == null)
                return;

            //3. 创建
            _Graph = PlayableGraph.Create();
            _ClipPlayable = AnimationClipPlayable.Create(_Graph, _Clip);
            _GraphOut = AnimationPlayableOutput.Create(_Graph, "Animation", _Animator);
            _GraphOut.SetSourcePlayable(_ClipPlayable);
            _ClipLen = _GetAnimClipLength(_Clip);
            if (!_AutoUpdate)
                _ClipPlayable.SetSpeed(0);
            _Graph.Play();
        }

        public bool IsValid()
        {
            if (_Animator == null || _Clip == null)
                return false;
            return true;
        }

        public void SetAnimProgress(float percent)
        {
            if (_Animator == null || _Clip == null)
                return;
            _ClipPlayable.SetTime(_ClipLen * percent);
        }

        public void SetAnimTime(float time)
        {
            if (_Animator == null || _Clip == null)
                return;
            _ClipPlayable.SetTime(time);
        }

        private static double _GetAnimClipLength(AnimationClip clip)
        {
            if (clip == null || clip.empty)
                return 0;

            float len = clip.length;
            float rate = clip.frameRate;
            if (rate < 0.0001f)
                return len;

            return Mathf.Round(len * rate) / (double)rate;
        }
    }
}
