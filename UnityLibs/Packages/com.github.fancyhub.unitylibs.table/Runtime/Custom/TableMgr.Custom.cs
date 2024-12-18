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

            _.LoadAll(lang);

            var list = _.Loc.List;
            List<(LocId, string)> ret = new List<(LocId, string)>(list.Count);
            foreach (var p in list)
            {
                ret.Add((new LocId(p.Id), p.Val));
            }
            return ret;
        }

        public static List<(LocId key, string tran)> EdLoadTranslation(string lang)
        {
            ITableReaderCreator table_reader_creator = new TableReaderCsvTextCreator(TableMgr.CDataDir);            
            TableTLoc table = new TableTLoc();
            if (!table_reader_creator.CreateTableReader(table.SheetName, lang, out var reader))
                return new List<(LocId key, string tran)>();

            if (!table.LoadFromCsv(reader))
                return new List<(LocId key, string tran)>();

            var list = table.List;
            List<(LocId, string)> ret = new List<(LocId, string)>(list.Count);

            foreach (var p in list)
            {
                ret.Add((new LocId(p.Id), p.Val));
            }
            return ret;             
        }

        private void OnInstCreate()
        {
            LoadAll();
        }
    }
}
