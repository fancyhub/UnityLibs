//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
using LocStr = FH.LocKey;

namespace FH{

    public abstract partial class Table
    {
        public virtual void BuildMap() { }
        public abstract bool IsMutiLang { get; }
        public abstract string SheetName { get; }
    }

    [System.Serializable]
    public abstract partial class Table<TTableItem> : Table where TTableItem : class
    {
        public List<TTableItem> List = new List<TTableItem>();
    }


    [System.Serializable]
    public sealed partial class TableTItemData: Table<TItemData>
    {
        public override bool IsMutiLang => false;
        public override string SheetName => "ItemData";
    

        [System.NonSerialized] public Dictionary<int, TItemData> Map;        


        public TItemData Find(int Id)
        {
            if (Map == null)
            {
                Log.E("TableTItemData's map is null");
                return null;
            }
            ;
            Map.TryGetValue(Id, out var ret);
            if(Id!=default);
            Log.Assert(ret != null, "can't find {0} in TableTItemData", Id);
            return ret;
        }    


        public override void BuildMap()
        {
            if (Map == null)
                Map = new (List.Count);
            Map.Clear();
            foreach (var p in List)
            {
                Map[p.Id] = p;
            }
        }
	}

    [System.Serializable]
    public sealed partial class TableTLoc: Table<TLoc>
    {
        public override bool IsMutiLang => true;
        public override string SheetName => "Loc";
    

        [System.NonSerialized] public Dictionary<int, TLoc> Map;        


        public TLoc Find(int Id)
        {
            if (Map == null)
            {
                Log.E("TableTLoc's map is null");
                return null;
            }
            ;
            Map.TryGetValue(Id, out var ret);
            if(Id!=default);
            Log.Assert(ret != null, "can't find {0} in TableTLoc", Id);
            return ret;
        }    


        public override void BuildMap()
        {
            if (Map == null)
                Map = new (List.Count);
            Map.Clear();
            foreach (var p in List)
            {
                Map[p.Id] = p;
            }
        }
	}

    [System.Serializable]
    public sealed partial class TableTTestComposeKey: Table<TTestComposeKey>
    {
        public override bool IsMutiLang => false;
        public override string SheetName => "TestComposeKey";
    

        [System.NonSerialized] public Dictionary<(uint Id, int Level), TTestComposeKey> Map;        



        public TTestComposeKey Find(uint Id, int Level)
        {
            if (Map == null)
            {
                Log.E("TableTTestComposeKey's map is null");
                return null;
            }
            Map.TryGetValue((Id,Level), out var ret);
            Log.Assert(ret != null, "can't find {0} in TableTTestComposeKey", (Id,Level));
            return ret;
        }    


        public override void BuildMap()
        {
            if (Map == null)
                Map = new (List.Count);
            Map.Clear();
            foreach (var p in List)
            {
                Map[(p.Id,p.Level)] = p;
            }
        }
	}

    [System.Serializable]
    public sealed partial class TableMgr
    {  

		public TableTItemData ItemData = new();
		public TableTLoc Loc = new();
		public TableTTestComposeKey TestComposeKey = new();
        
        private List<Table> _AllTables;
        public List<Table> AllTables
        {            
            get {
                if(_AllTables!=null)
                    return _AllTables;
                _AllTables = new List<Table>(3);            

				_AllTables.Add(ItemData);
				_AllTables.Add(Loc);
				_AllTables.Add(TestComposeKey);

                return _AllTables;
            }
        }
    }
}
