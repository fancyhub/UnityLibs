using System;
using System.Collections.Generic;
namespace FH
{
    [UnityEngine.ExecuteAlways]
    [UnityEngine.ExecuteInEditMode]
    public sealed class TimeUtilUpdater : UnityEngine.MonoBehaviour
    {
        public void Start() { TimeUtil.SetFrameCount(UnityEngine.Time.frameCount); }

        public void Update() { TimeUtil.SetFrameCount(UnityEngine.Time.frameCount); }

        public void FixedUpdate() { TimeUtil.SetFrameCount(UnityEngine.Time.frameCount); }

        private static TimeUtilUpdater _inst;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [UnityEngine.RuntimeInitializeOnLoadMethod]
        public static void _InitUpdater()
        {
            if (_inst != null)
                return;
            UnityEngine.GameObject obj = new UnityEngine.GameObject("TimeUtilUpdater");
            if (UnityEngine.Application.isPlaying)
                UnityEngine.GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            _inst = obj.AddComponent<TimeUtilUpdater>();
        }
    }
}
