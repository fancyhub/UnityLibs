/*************************************************************************************
 * Author  : cunyu.fan
 * Title   : UIView Generator Error Collector
 * Desc    : 收集代码生成过程中的错误和警告，由最外层统一展示。
*************************************************************************************/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public sealed class EdUIViewGenError
    {
        public enum EType
        {
            Error,
            Warning,
        }

        public sealed class Item
        {
            public EType Type;
            public string Message;
            public Object Context;
        }

        private readonly List<Item> _Items = new List<Item>();

        public IReadOnlyList<Item> Items => _Items;
        public bool HasError => _HasType(EType.Error);
        public bool HasWarning => _HasType(EType.Warning);
        public bool IsEmpty => _Items.Count == 0;

        public void AddError(string message, Object context = null)
        {
            _Add(EType.Error, message, context);
        }

        public void AddWarning(string message, Object context = null)
        {
            _Add(EType.Warning, message, context);
        }

        public void Log()
        {
            foreach (Item item in _Items)
            {
                if (item.Type == EType.Error)
                    Debug.LogError(item.Message, item.Context);
                else
                    Debug.LogWarning(item.Message, item.Context);
            }
        }

        public void ShowDialog(string title = "UIView生成结果")
        {
            if (IsEmpty)
                return;

            List<string> messages = new List<string>();
            foreach (Item item in _Items)
            {
                string prefix = item.Type == EType.Error ? "错误: " : "警告: ";
                messages.Add(prefix + item.Message);
            }

            EditorUtility.DisplayDialog(title, string.Join("\n\n", messages), "确认");
        }

        public void Clear()
        {
            _Items.Clear();
        }

        private void _Add(EType type, string message, Object context)
        {
            if (string.IsNullOrEmpty(message))
                return;

            _Items.Add(new Item()
            {
                Type = type,
                Message = message,
                Context = context,
            });
        }

        private bool _HasType(EType type)
        {
            foreach (Item item in _Items)
            {
                if (item.Type == type)
                    return true;
            }

            return false;
        }
    }
}
