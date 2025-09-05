
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
                EdCoroutineRunner.StartCoroutine(enumerator);
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
        static class EdCoroutineRunner
        {
            internal struct RoutineData
            {
                public int _CoroutineId;
                public IEnumerator _Enumerator;
            }

            private static int _IdGen = 0;
            private static Dictionary<int, RoutineData> _Dict = new();
            private static List<int> _EdListCoroutineIds = new();
            private static List<RoutineData> _EdRunningTempList = new();

            [UnityEditor.InitializeOnLoadMethod]
            static void Initialize()
            {
                UnityEditor.EditorApplication.update += _EdUpdate;
            }

            public static void StartCoroutine(IEnumerator enumerator)
            {
                RoutineData data = new RoutineData()
                {
                    _CoroutineId = ++_IdGen,
                    _Enumerator = _CreateEdRoutineExecutor(enumerator),
                };
                _Dict.Add(data._CoroutineId, data);
                _EdListCoroutineIds.Add(data._CoroutineId);
                return;
            }
             

            private static double _timer = 0;
            internal static void _EdUpdate()
            {
                var dt = Time.realtimeSinceStartupAsDouble - _timer;
                if (dt < 0.5f)
                    return;
                _timer = Time.realtimeSinceStartupAsDouble;


                _EdRunningTempList.Clear();
                for (int i = _EdListCoroutineIds.Count - 1; i >= 0; i--)
                {
                    var id = _EdListCoroutineIds[i];
                    if (!_Dict.TryGetValue(id, out var data))
                    {
                        _EdListCoroutineIds.RemoveAt(i);
                        continue;
                    }

                    _EdRunningTempList.Add(data);
                }

                foreach (var p in _EdRunningTempList)
                {
                    try
                    {
                        if (!p._Enumerator.MoveNext())
                        {
                            _Dict.Remove(p._CoroutineId);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            private static IEnumerator _CreateEdRoutineExecutor(IEnumerator orig)
            {

                if (orig == null)
                    yield break;

                System.Collections.Generic.Queue<IEnumerator> queue = new();
                queue.Enqueue(orig);

                for (; ; )
                {
                    if (queue.Count == 0)
                        break;

                    var top = queue.Peek();
                    if (top == null)
                    {
                        queue.Dequeue();
                        continue;
                    }

                    if (!top.MoveNext())
                    {
                        queue.Dequeue();
                        continue;
                    }

                    object current = orig.Current;
                    if (current == null)
                    {
                        yield return null;
                    }
                    else if (current is WaitForEndOfFrame waitForEndOfFrame)
                    {
                        yield return waitForEndOfFrame;
                    }
                    else if (current is WaitForFixedUpdate waitForFixedUpdate)
                    {
                        yield return waitForFixedUpdate;
                    }
                    else if (current is WaitForSeconds waitForSeconds)
                    {
                        //拿不到时间, 就直接写了
                        double endTime = Time.realtimeSinceStartupAsDouble + 1;

                        for (; ; )
                        {
                            if (endTime > Time.realtimeSinceStartupAsDouble)
                                yield return waitForSeconds;
                        }
                    }
                    else if (current is WaitForSecondsRealtime waitForSecondsRealtime)
                    {
                        double endTime = Time.realtimeSinceStartupAsDouble + waitForSecondsRealtime.waitTime;

                        for (; ; )
                        {
                            if (endTime > Time.realtimeSinceStartupAsDouble)
                                yield return waitForSecondsRealtime;
                        }
                    }
                    else if (current is IEnumerator otherEnumerator)
                    {
                        queue.Enqueue(otherEnumerator);
                        yield return otherEnumerator;
                    }
                    else if (current is AsyncOperation op)
                    {
                        for (; ; )
                        {
                            if (!op.isDone)
                                yield return null;
                        }
                    }
                    else
                    {
                        yield return current;
                    }
                }
            }
        }
#endif

    }
}
