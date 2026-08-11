using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public struct FieldResolverItem
    {
        public UnityEngine.Component TargetComp;
        public System.Type TargetType;

        public string FieldName;
        public bool SubView;
    }

    public interface IFieldResolver
    {
        public List<FieldResolverItem> CollectFields(GameObject prefab);
    }
}
