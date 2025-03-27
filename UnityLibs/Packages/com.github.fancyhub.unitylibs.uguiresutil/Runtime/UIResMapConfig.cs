using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    [Serializable]
    [CreateAssetMenu(fileName = "UIResMapConfig", menuName = "fancyhub/UI Res Map Config")]
    public class UIResMapConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string CPath = "Assets/Res/UI/Config/UIResMapConfig.asset";
        public const string CEditorPATH = CPath;

        public UIResMap Config = new UIResMap();

        private static UIResMap _GlobalResMap = null;

        public void OnAfterDeserialize()
        {
            Config.BuildDict();
        }


        public void OnBeforeSerialize()
        {
        }


        public static string FindSprite(string name)
        {
            var res_map = _LoadGlobalMap();
            if (res_map == null)
            {
                return null;
            }
            return res_map.FindSpritePath(name);
        }

        public static string FindTexture(string name)
        {
            var res_map = _LoadGlobalMap();
            if (res_map == null)
            {
                return null;
            }
            return res_map.FindTexturePath(name);
        }

        public static string FindPrefab(string name)
        {
            var res_map = _LoadGlobalMap();
            if (res_map == null)
            {
                return null;
            }
            return res_map.FindPrefabPath(name);
        }

        public static void Reload()
        {
            _GlobalResMap = null;
            _LoadGlobalMap();
        }

        private static UIResMap _LoadGlobalMap()
        {
            if (_GlobalResMap != null)
                return _GlobalResMap;

#if UNITY_EDITOR
            bool use_editor = true;
            if (use_editor)
            {
                _GlobalResMap = new UIResMap();
                _GlobalResMap.EdCollectAll();
                return _GlobalResMap;
            }
#endif

            //不持有该 ref,  就让他自然卸载
            var res_ref = ResMgr.Load(CPath);
            var asset = res_ref.Get<UIResMapConfig>();
            if (asset != null)
                _GlobalResMap = asset.Config;

            return _GlobalResMap;
        }

#if UNITY_EDITOR
        //[Sirenix.OdinInspector.Button]
        public void EdBuild()
        {
            Config = new UIResMap();
            Config.EdCollectAll();
        }
#endif
    }


    [Serializable]
    public class UIResMap
    {
        //private const string CEditorRoot = "Assets/Resources/";
        //private const string CSpriteRoot = "UI/Sprite";
        //private const string CEditorSpriteRoot = CEditorRoot + CSpriteRoot;
        //private const string CTextureRoot = "UI/Texture";
        //private const string CEditorTextureRoot = CEditorRoot + CTextureRoot;
        //private const string CPrefabRoot = "UI/Prefab";
        //private const string CEditorPrefabRoot = CEditorRoot + CPrefabRoot;

        private const string CEditorRoot = "";
        private const string CSpriteRoot = "Assets/Res/UI/Sprite";
        private const string CEditorSpriteRoot = CEditorRoot + CSpriteRoot;
        private const string CTextureRoot = "Assets/Res/UI/Texture";
        private const string CEditorTextureRoot = CEditorRoot + CTextureRoot;
        private const string CPrefabRoot = "Assets/Res/UI/Prefab";
        private const string CEditorPrefabRoot = CEditorRoot + CPrefabRoot;

        private const bool CIncludeExt = true;

        private TagLog Logger => TagLog.Create("UIResMap", LogLvl);

        [Serializable]
        public class UIResItem
        {
            public string name;
            public string dir;
            public string ext;

            public string FormatPath(string root)
            {
                if (!string.IsNullOrEmpty(ext))
                {
                    if (string.IsNullOrEmpty(dir))
                        return $"{root}/{name}{ext}";
                    return $"{root}/{dir}/{name}{ext}";
                }
                else
                {
                    if (string.IsNullOrEmpty(dir))
                        return $"{root}/{name}";
                    return $"{root}/{dir}/{name}";
                }
            }
        }

        public ELogLvl LogLvl = ELogLvl.Info;
        public List<UIResItem> sprite = new List<UIResItem>();
        public List<UIResItem> texture = new List<UIResItem>();
        public List<UIResItem> prefab = new List<UIResItem>();

        public Dictionary<string, UIResItem> sprite_dict;
        public Dictionary<string, UIResItem> texture_dict;
        public Dictionary<string, UIResItem> prefab_dict;

        public string FindSpritePath(string name)
        {
            if (string.IsNullOrEmpty(name) || sprite_dict == null)
                return null;
            if (!sprite_dict.TryGetValue(name, out var item))
                return null;

            return item.FormatPath(CSpriteRoot);

        }

        public string FindTexturePath(string name)
        {
            if (string.IsNullOrEmpty(name) || texture_dict == null)
                return null;
            if (!texture_dict.TryGetValue(name, out var item))
                return null;
            return item.FormatPath(CTextureRoot);
        }

        public string FindPrefabPath(string name)
        {
            if (string.IsNullOrEmpty(name) || prefab_dict == null)
                return null;
            if (!prefab_dict.TryGetValue(name, out var item))
                return null;
            return item.FormatPath(CPrefabRoot);
        }

#if UNITY_EDITOR
        public bool EdCollectAll()
        {
            bool ret = true;
            ret = _EdCollectSprite() && ret;
            ret = _EdCollectTexture() && ret;
            ret = _EdCollectPrefab() && ret;

            return ret;
        }


        private bool _EdCollectSprite()
        {
            Logger.D("开始 EdCollectSprite");
            bool has_error = false;
            sprite.Clear();
            string[] all_guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new string[] { CEditorSpriteRoot });
            foreach (var guid in all_guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                if (objs.Length != 2)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    var main_obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
                    Logger.E(main_obj, "一个图片多Sprite {0}", name);
                    has_error = true;
                    continue;
                }

                var item = _Parse2Item(path, CEditorSpriteRoot, CIncludeExt);
                if (item != null)
                    sprite.Add(item);
            }

            Logger.D("结束 EdCollectSprite");

            bool ret = _BuildDictSprite() && !has_error;
            return ret;
        }

        private bool _EdCollectTexture()
        {
            Logger.D("开始 EdCollectTexture");
            texture.Clear();
            string[] all_guids = UnityEditor.AssetDatabase.FindAssets("t:Texture", new string[] { CEditorTextureRoot });
            foreach (var guid in all_guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var item = _Parse2Item(path, CEditorTextureRoot, CIncludeExt);
                if (item != null)
                    texture.Add(item);
            }
            Logger.D("结束 EdCollectTexture");
            return _BuildDictTexture();
        }


        private bool _EdCollectPrefab()
        {
            Logger.D("开始 EdCollectPrefab");
            prefab.Clear();
            string[] all_guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { CEditorPrefabRoot });
            foreach (var guid in all_guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var item = _Parse2Item(path, CEditorPrefabRoot, CIncludeExt);
                if (item != null)
                    prefab.Add(item);
            }
            Logger.D("结束 EdCollectPrefab");
            return _BuildDictPrefab();
        }


        private static UIResItem _Parse2Item(string file_path, string editor_root, bool include_ext)
        {
            string file_name_with_ext = System.IO.Path.GetFileName(file_path);
            string file_name = System.IO.Path.GetFileNameWithoutExtension(file_path);
            string file_ext = System.IO.Path.GetExtension(file_path);

            file_path = file_path.Substring(0, file_path.Length - file_name_with_ext.Length);
            string inner_dir = "";
            if (file_path != editor_root)
                inner_dir = file_path.Substring(editor_root.Length);

            inner_dir = inner_dir.Trim('/');

            UIResItem ret = new UIResItem();
            ret.name = file_name;
            ret.dir = inner_dir;
            if (include_ext)
                ret.ext = file_ext;
            return ret;
        }
#endif


        public bool BuildDict()
        {
            bool ret = true;
            ret = _BuildDictSprite() && ret;
            ret = _BuildDictTexture() && ret;
            ret = _BuildDictPrefab() && ret;
            return ret;
        }

        private bool _BuildDictSprite()
        {
            Logger.D("开始 BuildDictSprite");
            bool has_error = false;
            sprite_dict = new Dictionary<string, UIResItem>(sprite.Count);
            foreach (var p in sprite)
            {
                if (!sprite_dict.TryAdd(p.name, p))
                {
                    has_error = true;
                    Logger.E("Sprite 重名, {0}", p.name);
                }
            }
            Logger.D("结束 BuildDictSprite {0}", sprite.Count);
            return !has_error;
        }
        private bool _BuildDictTexture()
        {
            Logger.D("开始 BuildDictTexture");

            bool has_error = false;
            texture_dict = new Dictionary<string, UIResItem>(texture.Count);
            foreach (var p in texture)
            {
                if (!texture_dict.TryAdd(p.name, p))
                {
                    has_error = true;
                    Logger.E("Texture 重名, {0}", p.name);
                }
            }
            Logger.D("结束 BuildDictTexture {0}", sprite.Count);
            return !has_error;
        }

        private bool _BuildDictPrefab()
        {
            Logger.D("开始 BuildDictPrefab");

            bool has_error = false;
            prefab_dict = new Dictionary<string, UIResItem>(prefab.Count);
            foreach (var p in prefab)
            {
                if (!prefab_dict.TryAdd(p.name, p))
                {
                    has_error = true;
                    Logger.E("Prefab 重名, {0}", p.name);
                }
            }
            Logger.D("结束 BuildDictPrefab {0}", sprite.Count);
            return !has_error;
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(UIResMapConfig))]
    public class UIResMapAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Collect"))
            {
                ((UIResMapConfig)target).EdBuild();
            }
        }
    }

    public class UIResMapBuilder : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            // 构建 server 不管
            if (UnityEditor.EditorUserBuildSettings.standaloneBuildSubtarget == UnityEditor.StandaloneBuildSubtarget.Server)
            {
                Debug.Log("UIResMap Skip in server===========================");
                return;
            }

            UnityEditor.AssetDatabase.MakeEditable(UIResMapConfig.CEditorPATH);
            Debug.Log("UIResMap 生成开始===========================");
            UIResMapConfig asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UIResMapConfig>(UIResMapConfig.CEditorPATH);
            if (!asset.Config.EdCollectAll())
            {
                throw new UnityEditor.Build.BuildFailedException("Build Res Map Faild, 查看前面的错误");
            }

            UnityEditor.AssetDatabase.ForceReserializeAssets(new string[] { UIResMapConfig.CEditorPATH }, UnityEditor.ForceReserializeAssetsOptions.ReserializeAssets);
            //AssetDatabase.SaveAssetIfDirty(asset);
            Debug.Log("UIResMap 生成结束===========================");
        }
    }
#endif
}
