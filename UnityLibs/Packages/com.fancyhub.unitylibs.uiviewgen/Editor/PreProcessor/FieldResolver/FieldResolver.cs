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
        public UnityEngine.Object TargetObject;
        public System.Type TargetType;

        public string FieldName;
        public bool SubView;

        public GameObject GetTargetGameObject()
        {
            if (TargetObject == null)
                return null;
            if (TargetObject is GameObject go)
                return go;
            else if(TargetObject is Component comp)
                return comp.gameObject;
            return null;
        }
    }

    public interface IFieldResolver
    {
        public List<FieldResolverItem> CollectFields(GameObject prefab);
    }
}
