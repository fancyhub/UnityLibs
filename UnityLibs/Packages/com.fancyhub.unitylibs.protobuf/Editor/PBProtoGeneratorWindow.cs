/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FH.Protobuf.Ed
{
    public sealed class PBProtoGeneratorWindow : EditorWindow
    {
        private const string CProtoDirKey = "FH.PBProtoGenerator.ProtoDir";
        private const string COutputDirKey = "FH.PBProtoGenerator.OutputDir";
        private const string CModeKey = "FH.PBProtoGenerator.Mode";
        private const string CMemberNameStyleKey = "FH.PBProtoGenerator.MemberNameStyle";

        private string _protoDir;
        private string _outputDir;
        private PBProtoCodeGenMode _mode;
        private PBProtoMemberNameStyle _memberNameStyle;

        [MenuItem("Tools/FancyHub/Protobuf/Generate C#")]
        public static void Open()
        {
            PBProtoGeneratorWindow window = GetWindow<PBProtoGeneratorWindow>();
            window.titleContent = new GUIContent("Protobuf");
            window.minSize = new Vector2(520, 180);
            window.Show();
        }

        private void OnEnable()
        {
            _protoDir = EditorPrefs.GetString(CProtoDirKey, "Assets");
            _outputDir = EditorPrefs.GetString(COutputDirKey, "Assets/Scripts/Generated/Protobuf");
            _mode = (PBProtoCodeGenMode)EditorPrefs.GetInt(CModeKey, (int)PBProtoCodeGenMode.ForceGenerate);
            _memberNameStyle = (PBProtoMemberNameStyle)EditorPrefs.GetInt(CMemberNameStyleKey, (int)PBProtoMemberNameStyle.KeepProtoName);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawDirectoryField("Proto Dir", ref _protoDir);
            DrawDirectoryField("Output Dir", ref _outputDir);
            _mode = (PBProtoCodeGenMode)EditorGUILayout.EnumPopup("Mode", _mode);
            _memberNameStyle = (PBProtoMemberNameStyle)EditorGUILayout.EnumPopup("Member Name", _memberNameStyle);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Generate", GUILayout.Width(120), GUILayout.Height(28)))
                    Generate();
            }
        }

        private static void DrawDirectoryField(string label, ref string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.TextField(label, value);
                if (GUILayout.Button("...", GUILayout.Width(32)))
                {
                    string start = ResolveStartDir(value);
                    string selected = EditorUtility.OpenFolderPanel(label, start, string.Empty);
                    if (!string.IsNullOrEmpty(selected))
                        value = ToProjectRelativePath(selected);
                }
            }
        }

        private void Generate()
        {
            try
            {
                EditorPrefs.SetString(CProtoDirKey, _protoDir);
                EditorPrefs.SetString(COutputDirKey, _outputDir);
                EditorPrefs.SetInt(CModeKey, (int)_mode);
                EditorPrefs.SetInt(CMemberNameStyleKey, (int)_memberNameStyle);


                var proj = PBProtoProj.LoadDirectory(_protoDir);
                if (proj.Files.Count == 0)
                {
                    Debug.LogWarning("No proto files found: " + _protoDir);
                    return;
                }

                int count = PBProtoCodeGenerator.Generate(proj, _outputDir, new PBProtoCodeGenOptions
                {
                    Mode = _mode,
                    MemberNameStyle = _memberNameStyle,
                });
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Protobuf", "Generated " + count + " file(s).", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Protobuf Error", ex.Message, "OK");
            }
        }

        private static string ResolveStartDir(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Application.dataPath;

            string full = Path.GetFullPath(path);
            return Directory.Exists(full) ? full : Application.dataPath;
        }

        private static string ToProjectRelativePath(string fullPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string normalized = Path.GetFullPath(fullPath);
            if (!normalized.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                return normalized;

            Uri rootUri = new Uri(AppendDirectorySeparator(projectRoot));
            Uri pathUri = new Uri(normalized);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                return path + Path.DirectorySeparatorChar;
            return path;
        }
    }
}
