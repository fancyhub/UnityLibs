using System;
using System.Collections.Generic;
using System.IO;

namespace FH.UI.ViewGenerate.Ed
{
    public interface ICodeExporter
    {
        public void Export(EdUIViewGenContext context);
    }
}
