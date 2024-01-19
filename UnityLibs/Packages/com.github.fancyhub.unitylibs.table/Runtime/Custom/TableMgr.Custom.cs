using System;
using System.Collections.Generic;

namespace FH
{
    public partial class TableMgr
    {
        private void OnInstCreate()
        {
            AddPostProcesser<TLoc>(_PP_Loc);
        }

        private void OnAllLoaded()
        {

        }

        private void _PP_Loc(Table table)
        {            
        }
    }
}
