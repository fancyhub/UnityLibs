using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17 16:33:30
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.AssetBundleManager.Builder
{
    /// <summary>
    /// Bundle Object Resource Type
    /// </summary>
    public enum EAssetObjType
    {
        none = 0,

        gameobject = 1,
        text,

        /// <summary>
        /// Texture , *.png,*.tga,*.jpg,*.dds
        /// 名字一样，后缀名不一样，加载的结果可能不是你想要的
        /// 比如：a.png,a.jpg, 加载的时候，可能会加载a.jpg，得不到你想要的a.png
        /// 所以在一个目录里面，名字最好不一样
        /// </summary>
        texture, // Texture, *.png,*.tga,

        /// <summary>
        /// spriteatlasv2
        /// </summary>
        altas,

        /// <summary>
        /// ScriptableObject, *.asset
        /// </summary>
        script_obj,

        /// <summary>
        /// AudioClip, *.wav,*.ogg,在同一个目录下，不要出现名字中仅仅后缀不一样的两个文件，比如a.wav,a.ogg
        /// </summary>
        audio,


        video,

        /// <summary>
        /// Font, *.ttf
        /// </summary>
        font,

        /// <summary>
        /// Mesh, *.mesh
        /// </summary>
        mesh,

        /// <summary>
        /// Material,*.mat
        /// </summary>
        material,

        /// <summary>
        /// Shader, *.shader
        /// </summary>
        shader,

        /// <summary>
        /// *.shadervariants
        /// </summary>
        shader_var_collection,

        /// <summary>
        /// AnimationClip, *.anim
        /// </summary>
        animation,

        /// <summary>
        /// RuntimeAnimatorController, *.controller
        /// </summary>
        anim_controller,

        /// <summary>
        /// 目前只在编辑器模式下使用
        /// 因为不能loadAsset的类型为 scene
        /// </summary>
        scene,


        anim_mask,

        compute,

        giparams,

        shadergraph,
        terrainlayer,
        mixer,          //音频的混合资源
    }

    public static class AssetObjType
    {
        private static Dictionary<string, EAssetObjType> _dict = new Dictionary<string, EAssetObjType>()
        {
            {".prefab", EAssetObjType.gameobject },
            {".fbx", EAssetObjType.gameobject },
            {".obj", EAssetObjType.gameobject },

            {".mask", EAssetObjType.anim_mask },

            {".asset", EAssetObjType.script_obj },
            {".playable", EAssetObjType.script_obj },

            {".unity", EAssetObjType.scene },

            {".anim", EAssetObjType.animation },
            {".controller", EAssetObjType.anim_controller },

            {".ogg", EAssetObjType.audio },
            {".mp3", EAssetObjType.audio },
            {".wav", EAssetObjType.audio },

            {".mp4", EAssetObjType.video },

            {".mat", EAssetObjType.material },
            {".cginc", EAssetObjType.shader },
            {".shader", EAssetObjType.shader },
            {".shadergraph", EAssetObjType.shadergraph },
            {".terrainlayer", EAssetObjType.terrainlayer },

            {".compute", EAssetObjType.compute },
            {".shadervariants", EAssetObjType.shader_var_collection },

            {".png", EAssetObjType.texture },
            {".tga", EAssetObjType.texture },
            {".jpg", EAssetObjType.texture },
            {".tif", EAssetObjType.texture },
            {".exr", EAssetObjType.texture },
            {".hdr", EAssetObjType.texture },
            {".cubemap", EAssetObjType.texture },
            {".bmp", EAssetObjType.texture },
            {".psd", EAssetObjType.texture },
            {".dds", EAssetObjType.texture },
            {".rendertexture", EAssetObjType.texture },

            {".txt", EAssetObjType.text },
            {".xml", EAssetObjType.text },
            {".bytes", EAssetObjType.text },
            {".json", EAssetObjType.text },

            {".ttf", EAssetObjType.font },
            {".otf", EAssetObjType.font },
            {".fontsettings", EAssetObjType.font },

            {".mesh", EAssetObjType.mesh },

            {".giparams", EAssetObjType.giparams },

            {".mixer", EAssetObjType.mixer },
            {".spriteatlasv2",EAssetObjType.altas  },
            {".spriteatlas",EAssetObjType.altas  },
        };

        public static EAssetObjType GetObjType(string path)
        {
            string file_ext = System.IO.Path.GetExtension(path);
            file_ext = file_ext.ToLower();

            bool succ = _dict.TryGetValue(file_ext, out EAssetObjType ret);
            if (succ) 
                return ret;
            return EAssetObjType.none;
        }
    }
}
