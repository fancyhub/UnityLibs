/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;

namespace FH.UI.ViewGenerate.Ed
{
    public interface ICodeAnalyser
    {
        public List<EdUIViewDesc> ParseAll(string code_folder);
        public EdUIViewDesc Parse(string code_path);
    }
}
