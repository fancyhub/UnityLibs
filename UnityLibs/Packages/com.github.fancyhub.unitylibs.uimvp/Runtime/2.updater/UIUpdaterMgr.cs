/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/3/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH.UI
{
    public sealed class UIUpdaterMgr
    {
        private struct Data
        {
            private readonly int Type;
            private readonly CPtr<IUIUpdater> UIUpdater;
            private readonly ActionUIUpdate Action;
            public readonly int ID;

            public Data(int id, IUIUpdater u)
            {
                Type = 1;
                ID = id;
                UIUpdater = new CPtr<IUIUpdater>(u);
                Action = null;
            }

            public Data(int id, ActionUIUpdate action)
            {
                Type = 2;
                ID = id;
                UIUpdater = default;
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
                            if (Action == null)
                                return "NULL";
                            return Action.GetType();
                        }
                }
            }

            public bool Update()
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
                            updater.OnUIUpdate();
                            return true;
                        }

                    case 2:
                        {
                            if (Action == null)
                                return false;
                            return Action() == EUIUpdateResult.Continue;
                        }
                }
            }
        }

        private LinkedList<Data> _List = new LinkedList<Data>();
        private Dictionary<int, LinkedListNode<Data>> _Dict = new();

        public bool AddUpdate(IUIUpdater updater)
        {
            if (updater == null)
            {
                UILog._.E("param updater is null");
                return false;
            }

            int id = updater.Id;
            if (_Dict.ContainsKey(id))
            {
                UILog._.W("updater is alread in update list, {0}:{1}", updater.Id, updater.GetType());
                return true;
            }

            var node = _List.ExtAddLast(new Data(id, updater));
            _Dict.Add(id, node);
            return true;
        }

        public int AddUpdate(ActionUIUpdate action)
        {
            if (action == null)
            {
                UILog._.E("param action is null");
                return 0;
            }

            int id = UIElementID.Next;
            var node = _List.ExtAddLast(new(id, action));
            _Dict.Add(id, node);
            return id;
        }

        public bool RemoveUpdate(int id)
        {
            if (id == 0)
                return false;

            if (!_Dict.TryGetValue(id, out var node))
            {
                UILog._.W("can't find updater {0}", id);
                return false;
            }

            _Dict.Remove(id);
            UILog._.D("remove updater {0}:{1}", id, node.Value.GetDataType());
            node.Value = default;
            return true;
        }

        public void Update()
        {
            var node = _List.First;
            for (; ; )
            {
                if (node == null)
                    return;
                Data data = node.Value;
                var cur = node;
                node = node.Next;

                if (!data.Update())
                {
                    cur.ExtRemoveFromList();
                    _Dict.Remove(data.ID);
                }
            }
        }
    }
}
