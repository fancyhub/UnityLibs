//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
using LocStr = FH.LocKey;

namespace FH{
    
    public partial class TableMgr
    {   

      
        public static List<TItemData> GetTItemDataList()
        {
            return FindTable<TItemData>()?.GetList<TItemData>();
        }
        

        public static TItemData GetTItemData(int Id)
        {
            return FindTable<TItemData>()?.Get<int,TItemData>(Id);
        }

        public static Dictionary<int, TItemData> GetTItemDataDict()
        {
            return FindTable<TItemData>()?.GetDict<int, TItemData>();
        }
        
      
        public static List<TLoc> GetTLocList()
        {
            return FindTable<TLoc>()?.GetList<TLoc>();
        }
        

        public static TLoc GetTLoc(int Id)
        {
            return FindTable<TLoc>()?.Get<int,TLoc>(Id);
        }

        public static Dictionary<int, TLoc> GetTLocDict()
        {
            return FindTable<TLoc>()?.GetDict<int, TLoc>();
        }
        
}
}
