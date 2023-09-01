#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.EventSetType1Sample
{
    public class Pub : MonoBehaviour
    {
        public static EventKey EventEnable = new EventKey(nameof(Pub), 1);
        public static EventKey EventXXX = new EventKey(nameof(Pub), 2);

        public void OnEnable()
        {
            EventEnable.ExtFire("hello Enable");
        }      
    }
}
#endif