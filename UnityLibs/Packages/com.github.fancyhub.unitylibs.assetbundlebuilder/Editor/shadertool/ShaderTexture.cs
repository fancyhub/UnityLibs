/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEditor;
using UnityEngine;


namespace FH.AssetBundleBuilder.Ed
{
    public sealed class ShaderTexture
    {
        private const string CDir = "Assets/fancyhub/ShaderTexture/";
        private const string CPathSimple2D = CDir + "Simple2D.tga";
        private const string CPathSimple2DNormalMap = CDir + "Simple2DNormalMap.tga";
        private const string CPathSimpleCubemap = CDir + "SimpleCubemap.cubemap";

        public Texture2D Simple2D;
        public Texture2D Simple2DNormalMap;
        public Cubemap SimpleCubemap;
        public Texture3D Simple3D;
        public Texture2DArray Simple2DArray;
        public CubemapArray SimpleCubemapArray;

        public static ShaderTexture Load()
        {
            ShaderTexture ret = new ShaderTexture();
            FileUtil.CreateDir(CDir);

            ret.Simple2D = AssetDatabase.LoadAssetAtPath<Texture2D>(CPathSimple2D);
            if (ret.Simple2D == null)
            {
                System.IO.File.WriteAllBytes(CPathSimple2D, Texture2D.whiteTexture.EncodeToTGA());
                AssetDatabase.ImportAsset(CPathSimple2D);

                ret.Simple2D = AssetDatabase.LoadAssetAtPath<Texture2D>(CPathSimple2D);
            }

            ret.Simple2DNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(CPathSimple2DNormalMap);
            if (ret.Simple2DNormalMap == null)
            {
                System.IO.File.WriteAllBytes(CPathSimple2DNormalMap, Texture2D.normalTexture.EncodeToTGA());
                AssetDatabase.ImportAsset(CPathSimple2DNormalMap);

                TextureImporter ti = AssetImporter.GetAtPath(CPathSimple2DNormalMap) as TextureImporter;
                ti.textureType = TextureImporterType.NormalMap;

                ret.Simple2DNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(CPathSimple2DNormalMap);
            }

            ret.SimpleCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(CPathSimpleCubemap);
            if (ret.SimpleCubemap == null)
            {
                ret.SimpleCubemap = new Cubemap(16, TextureFormat.RGBA32, false);
                AssetDatabase.CreateAsset(ret.SimpleCubemap, CPathSimpleCubemap);
            }
            return ret;
        }

        private ShaderTexture()
        {

        }
    }
}
