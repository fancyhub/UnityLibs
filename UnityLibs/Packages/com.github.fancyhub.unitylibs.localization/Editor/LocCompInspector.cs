using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{
    [CustomEditor(typeof(LocComp), true)]
    public class LocCompInspector : Editor
    {
        private int _LangIndex;
        public void OnEnable()
        {
            if (LocMgr.FuncLoader == null)
            {
                ITableReaderCreator table_reader_creator = new TableReaderCsvCreator(TableMgr.CDataDir);
                TableLoaderMgr table_loader_mgr = new TableLoaderMgr(table_reader_creator.CreateTableReader);

                table_loader_mgr.LoaderDict.TryGetValue(typeof(TLoc), out var info);

                LocMgr.FuncLoader = (lang) =>
                {
                    var table = info.Loader(lang);
                    if (table == null)
                        return null;

                    var list = table.GetList<TLoc>();
                    List<(LocId, string)> ret = new List<(LocId, string)>(list.Count);

                    foreach (var p in list)
                    {
                        ret.Add((new LocId(p.Id), p.Val));
                    }
                    return ret;
                };

                LocMgr.EdReloadAll();
            }

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
                string lang= LocLang.LangList[index];
                ((LocComp)target).EdDoLocalize(lang);
            }

            if (GUILayout.Button("Reload"))
            {
                LocMgr.EdReloadAll();
            }
            GUILayout.EndHorizontal();


            base.OnInspectorGUI();
        }
    }
}
