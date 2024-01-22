/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{

    public static class LocMgrEditorLoader
    {
        private static TableLoader _TableTranLoader;
        private static List<(LocId key, string tran)> EdLoadTranslation(string lang)
        {
            if (_TableTranLoader == null)
            {
                ITableReaderCreator table_reader_creator = new TableReaderCsvCreator(TableMgr.CDataDir);
                TableLoaderMgr table_loader_mgr = new TableLoaderMgr(table_reader_creator.CreateTableReader);
                table_loader_mgr.LoaderDict.TryGetValue(typeof(TLoc), out var info);

                _TableTranLoader = info.Loader;
            }

            var table = _TableTranLoader(lang);
            if (table == null)
                return null;

            var list = table.GetList<TLoc>();
            List<(LocId, string)> ret = new List<(LocId, string)>(list.Count);

            foreach (var p in list)
            {
                ret.Add((new LocId(p.Id), p.Val));
            }
            return ret;
        }

        public static void Init()
        {
            if (Application.isPlaying)
                return;

            if (LocMgr.FuncLoader != EdLoadTranslation)
            {
                LocMgr.FuncLoader = EdLoadTranslation;
                LocMgr.EdReloadAll();
            }
        }
    }

    [CustomEditor(typeof(LocComp), true)]
    public class LocCompInspector : Editor
    {
        private SerializedProperty _KeyProperty;
        private int _LangIndex;

        public virtual void OnEnable()
        {
            _KeyProperty = serializedObject.FindProperty("_LocKey");
            LocMgrEditorLoader.Init();
            ((LocComp)target).EdDoLocalize(LocLang.Lang);
            _LangIndex = LocLang.IndexOf(LocLang.Lang);
        }


        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();

            int index = EditorGUILayout.Popup("Lang", _LangIndex, LocLang.LangList);
            if (index != _LangIndex)
            {
                _LangIndex = index;
                string lang = LocLang.LangList[index];
                ((LocComp)target).EdDoLocalize(lang);
            }

            if (GUILayout.Button("Reload"))
            {
                LocMgr.EdReloadAll();
            }
            GUILayout.EndHorizontal();


            EditorGUILayout.PropertyField(_KeyProperty);
        }
    }
}
