using System;
using System.Collections.Generic;

namespace FH
{
    public partial class TableMgr
    {
        public static void Init(ELogLvl log_lvl, ITableReaderCreator table_reader_creator)
        {
            TableLog.SetMasks(log_lvl);
            TableLog.D("Init TableMgr");
            if (_ != null)
            {
                TableLog.E("TableMgr Is Inited, can't init twice");
                return;
            }

            if (table_reader_creator == null)
            {
                TableLog.E("Param table_reader_creator is null");
                return;
            }
            _ = new TableMgr(table_reader_creator);
            TableLog.D("TableMgr Init Succ");
        }

        public static List<(LocId key, string tran)> LoadTranslation(string lang)
        {
            if (_ == null)
            {
                TableLog.E("TableMgr Is Not Inited");
                return null;
            }

            _._LoaderMgr.LoaderDict.TryGetValue(typeof(TLoc), out var info);
            if (info.Loader == null)
            {
                TableLog.E("Cant Find Translation Loader {0}", nameof(TLoc));
                return null;
            }

            var table = info.Loader(lang);
            if (table == null)
            {
                TableLog.E("Load Translation Failed: {0}", lang);
                return null;
            }

            var list = table.GetList<TLoc>();
            List<(LocId, string)> ret = new List<(LocId, string)>(list.Count);

            foreach (var p in list)
            {
                ret.Add((new LocId(p.Id), p.Val));
            }
            return ret;
        }

        private void OnInstCreate()
        {
            AddPostProcesser<TLoc>(_PP_Loc);

            LoadAllTable();
        }


        private void OnAllLoaded()
        {

        }

        private void _PP_Loc(Table table)
        {
        }
    }
}
