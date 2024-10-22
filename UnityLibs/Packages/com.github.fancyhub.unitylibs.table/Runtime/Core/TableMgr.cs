using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    public partial class TableMgr
    {
        public const string CDataDir = "../ClientData/Table/";
        private static TableMgr _;

        private string _Lang;
        private bool _Loaded = false;
        public ITableReaderCreator TableReaderCreator;

        private TableMgr(ITableReaderCreator creator)
        {
            TableReaderCreator = creator;
            OnInstCreate();
        }

        public static void Destroy()
        {
            if (_ == null)
                return;
            var t = _;
            _ = null;
            t.TableReaderCreator?.CloseReader();
        }

        public void LoadAll(string lang = null, bool force_reload = false)
        {
            //先加载多语言
            if (lang != _Lang || force_reload)
            {
                if (lang != null)
                {
                    _Lang = lang;
                    foreach (var t in AllTables)
                    {
                        if (!t.IsMutiLang)
                            continue;

                        if (!TableReaderCreator.CreateTableReader(t.SheetName, _Lang, out var reader))
                            continue;

                        if (t.LoadFromCsv(reader))
                            t.BuildMap();
                    }
                }
            }

            //加载非多语言
            if (!_Loaded || force_reload)
            {
                _Loaded = true;
                foreach (var t in AllTables)
                {
                    if (t.IsMutiLang)
                        continue;

                    if (!TableReaderCreator.CreateTableReader(t.SheetName, null, out var reader))
                        continue;

                    if (t.LoadFromCsv(reader))
                        t.BuildMap();
                }
            }
        }

    }
}
