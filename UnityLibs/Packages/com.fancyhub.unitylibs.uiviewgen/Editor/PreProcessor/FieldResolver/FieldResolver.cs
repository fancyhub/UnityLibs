using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FH.UI.ViewGenerate.Ed
{
    public interface IFieldResolver
    {
        public List<EdUIField> CreateFields(EdUIViewGenContext context, EdUIView view);
    }
}
