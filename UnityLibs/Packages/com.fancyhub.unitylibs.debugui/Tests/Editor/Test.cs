using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace FH.DebugUI.EdTests
{

    public static class TestCmd
    {
        [ActionView("Test.Cmd", "Add")]
        public static void Add(int a, int b = 5)
        {
            Debug.Log($"{a} + {b} = {a + b}");
        }


        [ActionView("Test.Cmd", "Minus")]
        public static void Minus(int a, int b)
        {
            Debug.Log($"{a} - {b} = {a - b}");
        }

        [ActionView("Test.Cmd2.Cmd", "Minus")]
        public static void Minus2(int a, int b)
        {
            Debug.Log($"{a} - {b} = {a - b}");
        }

    }
}