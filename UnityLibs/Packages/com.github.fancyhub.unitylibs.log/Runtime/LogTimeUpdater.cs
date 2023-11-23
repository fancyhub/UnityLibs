using System;
using System.Collections.Generic;
namespace FH
{
    [UnityEngine.ExecuteAlways]
    [UnityEngine.ExecuteInEditMode]
    public sealed class LogTimeUpdater : UnityEngine.MonoBehaviour
    {
        public static int FrameCount;
        public void Start() { FrameCount = UnityEngine.Time.frameCount; }

        public void Update() { FrameCount = UnityEngine.Time.frameCount; }

        public void FixedUpdate() { FrameCount = UnityEngine.Time.frameCount; }

        private static LogTimeUpdater _inst;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [UnityEngine.RuntimeInitializeOnLoadMethod]
        public static void _InitUpdater()
        {
            if (_inst != null)
                return;
            UnityEngine.GameObject obj = new UnityEngine.GameObject("LogTimeUpdater");
            if (UnityEngine.Application.isPlaying)
                UnityEngine.GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            _inst = obj.AddComponent<LogTimeUpdater>();
        }
    }
}
