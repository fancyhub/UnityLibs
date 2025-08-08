using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    [RequireComponent(typeof(UIDocument))]
    public class DebugUIIEntry : MonoBehaviour
    {
        
        protected DebugUIIManager Mgr;
        protected UIDocument UIDocument;
        [Header("UGUI的Canvas 不能使用Overlay模式")]
        public List<DebugUICommandAsset> Commands;
        public void Awake()
        {
            Mgr = new DebugUIIManager();
            UIDocument = GetComponent<UIDocument>();
        }

        public virtual void Start()
        {
            foreach (var p in DebugUIUtil.FindMethodsWithAttribute<ActionViewAttribute>())
            {
                var actionView = new ActionView(p.method, p.attr.Name);
                Mgr.AddItem(p.attr.Path, actionView);

                Mgr.RegCommand($"{p.attr.Path}.{actionView.DebugUIItemName}", p.method);
            }


            if (Commands != null)
            {
                foreach (var p in Commands)
                {
                    foreach (var p2 in p.Commands)
                    {
                        Mgr.AddItem(p2.Path, new CommandView(p2, Mgr));
                    }
                }
            }

            var doc = Mgr.ShowInUIDocument(UIDocument);
            doc.CloseAction = _OnBtnCloseClick;
        }

        protected void OnEnable()
        {
            var doc = Mgr.ShowInUIDocument(UIDocument);
            doc.CloseAction = _OnBtnCloseClick;
        }

        private void _OnBtnCloseClick()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
