/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/12/27
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class UIImageGUIRenderer : MonoBehaviour
    {
        private Image _Image;
        private RectTransform _RectTransform;
        void Start()
        {
            _InitFields();
        }
   
        public void OnEnable()
        {
            _InitFields();
            _Image.enabled = false;
        }

        public void OnGUI()
        {
            var sprite = _Image.sprite;

            if (!GetSpriteCoords(sprite, out var coords))
                return;

            if (!_RectTransform.ExtToScreenNormalize2(out var screenRectNormal, out Vector2 screenSize))
                return;

            Rect rect = new Rect(Vector2.Scale(screenSize, screenRectNormal.position), Vector2.Scale(screenSize, screenRectNormal.size));

            GUI.DrawTextureWithTexCoords(rect, sprite.texture, coords, false);
        }

        private void _InitFields()
        {
            if (_RectTransform == null)
                _RectTransform = GetComponent<RectTransform>();
            if (_Image == null)
                _Image = GetComponent<Image>();
        }

        private static bool GetSpriteCoords(Sprite sprite, out Rect coords)
        {
            coords = new Rect(0, 0, 1, 1);
            if (sprite == null)
                return false;

            Texture2D tex = sprite.texture;
            if (tex == null)
                return false;

            try
            {
                coords = sprite.textureRect;
            }
            catch
            {
                return false;
            }

            int tex_width = tex.width;
            int tex_height = tex.height;

            coords.x /= tex_width;
            coords.width /= tex_width;
            coords.y /= tex_height;
            coords.height /= tex_height;
            return true;
        }
    }
}
