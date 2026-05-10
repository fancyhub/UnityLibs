
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/9/5
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public static class GlobalCoroutine
    {
        private static GlobalCoroutineHelper _Helper;

        public static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (enumerator == null)
                return null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorStartCoroutine(enumerator);
                return null;

            }
#endif
            if (_Helper == null)
            {
                GameObject obj = new GameObject(nameof(GlobalCoroutineHelper));
                GameObject.DontDestroyOnLoad(obj);
                _Helper = obj.AddComponent<GlobalCoroutineHelper>();
            }

            return _Helper.StartCoroutine(enumerator);
        }

        public static void StopCoroutine(Coroutine routine)
        {
            if (_Helper == null) return;
            _Helper.StopCoroutine(routine);
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            if (_Helper == null) return;
            _Helper.StopCoroutine(routine);
        }

        public static void StopAllCoroutines()
        {
            if (_Helper == null) return;
            _Helper.StopAllCoroutines();
        }

        private class GlobalCoroutineHelper : MonoBehaviour
        {

        }

#if UNITY_EDITOR

        private static EditorCoroutineInstructionProcessor _EditorInstructionProcessor = new EditorCoroutineInstructionProcessor();
        private static CoroutineExecutor _EditorExecutor = new CoroutineExecutor(_EditorInstructionProcessor);

        [UnityEditor.InitializeOnLoadMethod]
        static void Initialize()
        {
            UnityEditor.EditorApplication.update += _EdUpdate;
        }

        public static void EditorStartCoroutine(IEnumerator enumerator)
        {
            _EditorExecutor.Start(enumerator);
        }

        private static double _timer = 0;
        internal static void _EdUpdate()
        {
            var dt = Time.realtimeSinceStartupAsDouble - _timer;
            if (dt < 0.5f)
                return;
            _timer = Time.realtimeSinceStartupAsDouble;

            _EditorExecutor.Tick();
            _EditorInstructionProcessor.Update();
        }
#endif

    }


    public struct CoroutineExecutorHandler
    {
        public readonly int CoroutineId;
        private readonly CoroutineExecutor _Executor;

        public CoroutineExecutorHandler(CoroutineExecutor executor, int id)
        {
            CoroutineId = id;
            _Executor = executor;
        }

        public void Pause()
        {
            _Executor?.Pause(this);
        }

        public void Resume()
        {
            _Executor?.Resume(this);
        }

        public void TickOnce()
        {
            _Executor?.TickOnce(this);
        }
    }

    public enum ECoroutineInstructionResult
    {
        Continue,
        Pause,
    }

    public interface ICoroutineInstructionProcessor
    {
        public ECoroutineInstructionResult Process(CoroutineExecutorHandler handler, System.Object obj);
        public void OnCoroutineRemoved(CoroutineExecutorHandler handler);
    }

    public class EmptyCoroutineInstructionProcessor : ICoroutineInstructionProcessor
    {
        public ECoroutineInstructionResult Process(CoroutineExecutorHandler handler, System.Object obj)
        {
            return ECoroutineInstructionResult.Continue;
        }
        public void OnCoroutineRemoved(CoroutineExecutorHandler handler)
        {
        }
    }

#if UNITY_EDITOR 
    public class EditorCoroutineInstructionProcessor : ICoroutineInstructionProcessor
    {
        public struct TimeItem
        {
            public CoroutineExecutorHandler Handler;
            public double ExpireTime;
            public bool ShouldTickOnce;
        }

        public struct OpItem
        {
            public CoroutineExecutorHandler Handler;
            public AsyncOperation Op;
        }

        private LinkedList<TimeItem> _timeList = new LinkedList<TimeItem>();
        private LinkedList<OpItem> _asyncOperations = new LinkedList<OpItem>();

        public void Update()
        {
            {
                var now = Time.realtimeSinceStartupAsDouble;

                for (var node = _timeList.First; ;)
                {
                    if (node == null)
                        break;

                    var cur = node;
                    node = node.Next;

                    if (now < cur.Value.ExpireTime)
                        continue;
                    cur.Value.Handler.Resume();
                    if (cur.Value.ShouldTickOnce)
                        cur.Value.Handler.TickOnce();
                    _timeList.Remove(cur);
                }
            }

            {
                for (var node = _asyncOperations.First; ;)
                {
                    if (node == null)
                        break;
                    var cur = node;
                    node = node.Next;


                    if (!cur.Value.Op.isDone)
                        continue;

                    cur.Value.Handler.Resume(); //等待下一帧自然触发
                    _asyncOperations.Remove(cur);
                }
            }
        }

        public ECoroutineInstructionResult Process(CoroutineExecutorHandler handler, System.Object obj)
        {
            switch (obj)
            {
                case WaitForEndOfFrame waitForEndOfFrame:
                    _timeList.AddLast(new TimeItem
                    {
                        Handler = handler,
                        ExpireTime = Time.realtimeSinceStartupAsDouble + 0.0001f,
                        ShouldTickOnce = true,
                    });
                    return ECoroutineInstructionResult.Pause;

                case WaitForFixedUpdate waitForFixedUpdate:
                    _timeList.AddLast(new TimeItem
                    {
                        Handler = handler,
                        ExpireTime = Time.realtimeSinceStartupAsDouble + 0.0001f,
                        ShouldTickOnce = true,
                    });
                    return ECoroutineInstructionResult.Pause;

                case WaitForSeconds waitForSeconds:
                    //拿不到时间, 就直接写了
                    _timeList.AddLast(new TimeItem
                    {
                        Handler = handler,
                        ExpireTime = Time.realtimeSinceStartupAsDouble + 1,
                        ShouldTickOnce = false,
                    });
                    return ECoroutineInstructionResult.Pause;

                case WaitForSecondsRealtime waitForSecondsRealtime:
                    _timeList.AddLast(new TimeItem
                    {
                        Handler = handler,
                        ExpireTime = Time.realtimeSinceStartupAsDouble + waitForSecondsRealtime.waitTime,
                        ShouldTickOnce = false,
                    });
                    return ECoroutineInstructionResult.Pause;



                case AsyncOperation op:
                    _asyncOperations.AddLast(new OpItem()
                    {
                        Handler = handler,
                        Op = op,
                    });
                    return ECoroutineInstructionResult.Pause;

                default:
                    return ECoroutineInstructionResult.Continue;
            }
        }

        public void OnCoroutineRemoved(CoroutineExecutorHandler handler)
        {
        }
    }
#endif

    /// <summary>
    /// Coroutine 的执行器
    /// </summary>
    public class CoroutineExecutor
    {
        internal struct RoutineData
        {
            public int _CoroutineId;
            public IEnumerator _Enumerator;
            public bool _Pause;
        }

        private static int _IdGen = 0;
        private Dictionary<int, RoutineData> _Dict = new();
        private List<int> _ListCoroutineIds = new(); //添加的顺序
        private List<int> _RunningTempList = new();
        private ICoroutineInstructionProcessor _InstructionProcessor;

        public CoroutineExecutor(ICoroutineInstructionProcessor instructionProcessor)
        {
            _InstructionProcessor = instructionProcessor;
        }

        public CoroutineExecutorHandler Start(IEnumerator enumerator)
        {
            if (enumerator == null)
                return default;

            var id = _IdGen++;
            var handler = new CoroutineExecutorHandler(this, id);

            RoutineData data = new RoutineData()
            {
                _CoroutineId = id,
                _Pause = false,
                _Enumerator = _CreateInnerRoutine(enumerator, handler),
            };
            _Dict.Add(data._CoroutineId, data);
            _ListCoroutineIds.Add(data._CoroutineId);
            return handler;
        }

        public bool Stop(CoroutineExecutorHandler handler)
        {
            if (!_Dict.Remove(handler.CoroutineId))
                return false;

            _InstructionProcessor.OnCoroutineRemoved(handler);
            return true;
        }

        public bool IsRunning(CoroutineExecutorHandler handler)
        {
            return _Dict.ContainsKey(handler.CoroutineId);
        }

        public void Pause(CoroutineExecutorHandler handler)
        {
            if (!_Dict.TryGetValue(handler.CoroutineId, out var d))
                return;
            if (d._Pause)
                return;

            d._Pause = true;
            _Dict[handler.CoroutineId] = d;
        }

        public void Resume(CoroutineExecutorHandler handler)
        {
            if (!_Dict.TryGetValue(handler.CoroutineId, out var d))
                return;

            if (!d._Pause)
                return;
            d._Pause = false;
            _Dict[handler.CoroutineId] = d;
        }

        public void TickOnce(CoroutineExecutorHandler handler)
        {
            _RunOnce(handler.CoroutineId);
        }

        public void Tick()
        {
            _RunningTempList.Clear();
            for (int i = _ListCoroutineIds.Count - 1; i >= 0; i--)
            {
                var id = _ListCoroutineIds[i];
                if (!_Dict.TryGetValue(id, out var data))
                {
                    _ListCoroutineIds.RemoveAt(i);
                    continue;
                }

                if (data._Pause)
                    continue;

                _RunningTempList.Add(data._CoroutineId);
            }

            _RunningTempList.Reverse();
            foreach (var p in _RunningTempList)
            {
                _RunOnce(p);
            }
        }

        private void _RunOnce(int corId)
        {
            if (!_Dict.TryGetValue(corId, out var data))
                return;

            bool end = true;
            try
            {
                end = !data._Enumerator.MoveNext();
            }
            catch (Exception e)
            {
                end = true;
                Debug.LogException(e);
            }

            if (end)
            {
                _Dict.Remove(corId);
                _InstructionProcessor.OnCoroutineRemoved(new CoroutineExecutorHandler(this, corId));
            }
        }

        private IEnumerator _CreateInnerRoutine(IEnumerator orig, CoroutineExecutorHandler handler)
        {
            if (orig == null)
                yield break;

            System.Collections.Generic.Stack<IEnumerator> stack = new();
            stack.Push(orig);

            for (; ; )
            {
                if (stack.Count == 0)
                    break;

                var top = stack.Peek();
                if (top == null)
                {
                    stack.Pop();
                    continue;
                }

                if (!top.MoveNext())
                {
                    stack.Pop();
                    continue;
                }

                object current = top.Current;
                if (current == null)
                {
                    yield return current;
                }
                else if (current is IEnumerator otherEnumerator)
                {
                    stack.Push(otherEnumerator);
                    yield return current;
                }
                else if (_InstructionProcessor != null)
                {
                    var result = _InstructionProcessor.Process(handler, current);
                    if (result == ECoroutineInstructionResult.Pause)
                        Pause(handler);
                    yield return current;
                }
                else
                {
                    yield return current;
                }
            }
        }
    }
}
