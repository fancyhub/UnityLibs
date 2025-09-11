/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/3/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH.UI
{
    public interface IUIUpdater : ICPtr
    {
        public void OnUIUpdate(float dt);
    }

    public enum EUIUpdateResult
    {
        Continue,
        Stop,
    }

    public delegate EUIUpdateResult UIUpdateFunc(float dt);

    public sealed class UIUpdateList
    {
        private struct Data
        {
            public readonly int Type;
            public readonly CPtr<IUIUpdater> UIUpdater;
            public readonly UIUpdateFunc Func;
            public readonly System.Action<float> Action;

            public Data(IUIUpdater u)
            {
                Type = 1;
                UIUpdater = new CPtr<IUIUpdater>(u);
                Func = null;
                Action = null;
            }

            public Data(UIUpdateFunc action)
            {
                Type = 2;
                UIUpdater = default;
                Func = action;
                Action = null;
            }

            public Data(System.Action<float> action)
            {
                Type = 3;
                UIUpdater = default;
                Func = null;
                Action = action;
            }

            public System.Object GetDataType()
            {
                switch (Type)
                {
                    default:
                        return "NULL";
                    case 1:
                        {
                            var updater = UIUpdater.Val;
                            if (updater == null)
                                return "NULL";
                            return updater.GetType();
                        }

                    case 2:
                        {
                            if (Func == null)
                                return "NULL";
                            return Func.GetType();
                        }

                    case 3:
                        {
                            if (Action == null)
                                return "NULL";
                            return Action.GetType();
                        }
                }
            }

            public bool Update(float dt)
            {
                switch (Type)
                {
                    default:
                        return false;
                    case 1:
                        {
                            var updater = UIUpdater.Val;
                            if (updater == null)
                                return false;
                            updater.OnUIUpdate(dt);
                            return true;
                        }

                    case 2:
                        {
                            if (Func == null)
                                return false;
                            return Func(dt) == EUIUpdateResult.Continue;
                        }

                    case 3:
                        {
                            if (Action == null)
                                return false;
                            Action(dt);
                            return true;
                        }
                }
            }
        }

        private LinkedList<Data> _List = new LinkedList<Data>();

        public void Update(float dt)
        {
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return;
                Data data = node.Value;
                var cur = node;
                node = node.Next;

                if (!data.Update(dt))
                {
                    cur.ExtRemoveFromList();
                }
            }
        }

        public void Clear()
        {
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return;
                node.Value = default;
            }
        }

        #region IUIUpdater
        public bool Add(IUIUpdater updater)
        {
            if (updater == null)
            {
                UILog._.E("param updater is null");
                return false;
            }

            var node = _Find(updater);
            if (node != null)
            {
                UILog._.W("updater is alread in update list, {0}", updater.GetType());
                return true;
            }

            _List.ExtAddLast(new Data(updater));
            return true;
        }

        public bool Remove(IUIUpdater action)
        {
            if (action == null)
                return false;

            var node = _Find(action);
            if (node == null)
                return false;
            node.Value = default;
            return true;
        }

        public static UIUpdateList operator +(UIUpdateList list, IUIUpdater action)
        {
            if (list == null)
                return null;
            if (action == null)
                return list;
            list.Add(action);
            return list;
        }

        public static UIUpdateList operator -(UIUpdateList list, IUIUpdater action)
        {
            if (list == null)
                return null;
            list.Remove(action);
            return list;
        }

        private LinkedListNode<Data> _Find(IUIUpdater action)
        {
            if (action == null)
                return null;
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return null;
                if (node.Value.Type == 1 && node.Value.UIUpdater.Val == action)
                    return node;
                node = node.Next;
            }
        }
        #endregion

        #region UIUpdateFunc
        public bool Add(UIUpdateFunc action)
        {
            if (action == null)
            {
                UILog._.E("param action is null");
                return false;
            }

            var node = _Find(action);
            if (node != null)
            {
                UILog._.D("action has been added, can't add twice, {0}", action.GetType());
                return true;
            }

            node = _List.ExtAddLast(new(action));
            return true;
        }

        public bool Remove(UIUpdateFunc action)
        {
            if (action == null)
                return false;

            var node = _Find(action);
            if (node == null)
                return false;
            node.Value = default;
            return true;
        }

        public static UIUpdateList operator +(UIUpdateList list, UIUpdateFunc action)
        {
            if (list == null)
                return null;
            if (action == null)
                return list;
            list.Add(action);
            return list;
        }

        public static UIUpdateList operator -(UIUpdateList list, UIUpdateFunc action)
        {
            if (list == null)
                return null;
            list.Remove(action);
            return list;
        }

        private LinkedListNode<Data> _Find(UIUpdateFunc action)
        {
            if (action == null)
                return null;
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return null;
                if (node.Value.Func == action)
                    return node;
                node = node.Next;
            }
        }
        #endregion

        #region Action
        public bool Add(System.Action<float> action)
        {
            if (action == null)
            {
                UILog._.E("param action is null");
                return false;
            }

            var node = _Find(action);
            if (node != null)
            {
                UILog._.D("action has been added, can't add twice,{0}", action.GetType());
                return true;
            }

            _List.ExtAddLast(new(action));
            return true;
        }

        public bool Remove(System.Action<float> action)
        {
            if (action == null)
                return false;

            var node = _Find(action);
            if (node == null)
                return false;
            node.Value = default;
            return true;
        }


        public static UIUpdateList operator +(UIUpdateList list, System.Action<float> action)
        {
            if (list == null)
                return null;
            if (action == null)
                return list;
            list.Add(action);
            return list;
        }

        public static UIUpdateList operator -(UIUpdateList list, System.Action<float> action)
        {
            if (list == null)
                return null;
            list.Remove(action);
            return list;
        }

        private LinkedListNode<Data> _Find(System.Action<float> action)
        {
            if (action == null)
                return null;
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return null;
                if (node.Value.Action == action)
                    return node;
                node = node.Next;
            }
        }
        #endregion
              
    }
}
