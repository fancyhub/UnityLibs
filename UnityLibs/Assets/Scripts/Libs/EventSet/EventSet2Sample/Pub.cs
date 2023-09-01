#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.EventSet2Sample
{
    public class Pub : MonoBehaviour
    {
        public static readonly EventKey EventEnable = new EventKey(nameof(Pub), 1);
        public static readonly EventKey EventXXX = new EventKey(nameof(Pub), 2);

        public void OnEnable()
        {
            EventEnable.ExtFire("hello Enable");
        }

        public void OnDisable()
        {
            EventEnable.ExtFireDelay("hello Disable Delay");
        }
    }
}
#endif