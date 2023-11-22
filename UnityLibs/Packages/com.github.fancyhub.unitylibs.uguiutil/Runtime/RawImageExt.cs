using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    public static class RawImageExt
    { 
        public static void ExtScaleUV(this UnityEngine.UI.RawImage self, ScaleMode mode)
        {
            if (self == null)
                return;
            Texture texture = self.texture;
            if (texture == null)
                return;
            Vector2 texture_size = new Vector2(texture.width, texture.height);

            RectTransform rect_tran = self.GetComponent<RectTransform>();
            Vector2 screen_size = rect_tran.rect.size;
            if (Mathf.Abs(screen_size.x) < float.Epsilon || Mathf.Abs(screen_size.y) < float.Epsilon)
                return;

            switch (mode)
            {
                case ScaleMode.StretchToFill:
                    self.uvRect = new Rect(0, 0, 1, 1);
                    break;

                case ScaleMode.ScaleAndCrop:
                    {
                        float aspect_screen = screen_size.x / screen_size.y;
                        float aspect_texture = texture_size.x / texture_size.y;
                        if (aspect_screen > aspect_texture) //屏幕比较宽
                        {
                            float scale = screen_size.x / texture_size.x;
                            float new_texture_height = scale * texture_size.y;
                            float ratio = screen_size.y / new_texture_height;

                            self.uvRect = new Rect(0, (1 - ratio) * 0.5f, 1.0f, ratio);

                        }
                        else //屏幕比较高
                        {
                            float scale = screen_size.y / texture_size.y;
                            float new_texture_width = scale * texture_size.x;
                            float ratio = screen_size.x / new_texture_width;

                            self.uvRect = new Rect((1 - ratio) * 0.5f, 0, ratio, 1.0f);
                        }
                    }
                    break;

                case ScaleMode.ScaleToFit: //这种需要 贴图是Clamp 模式, 并且边缘最好是透明的
                    {
                        float aspect_screen = screen_size.x / screen_size.y;
                        float aspect_texture = texture_size.x / texture_size.y;
                        if (aspect_screen > aspect_texture) //屏幕比较宽
                        {
                            float scale = screen_size.y / texture_size.y;
                            float new_texture_width = scale * texture_size.x;
                            float ratio = screen_size.x / new_texture_width;
                            self.uvRect = new Rect((1 - ratio) * 0.5f, 0, ratio, 1.0f);
                        }
                        else //屏幕比较高
                        {
                            float scale = screen_size.x / texture_size.x;
                            float new_texture_height = scale * texture_size.y;
                            float ratio = screen_size.y / new_texture_height;
                            self.uvRect = new Rect(0, (1 - ratio) * 0.5f, 1.0f, ratio);
                        }
                    }
                    break;
                default:
                    self.uvRect = new Rect(0, 0, 1, 1);
                    break;
            }
        } 
    
    }
}
