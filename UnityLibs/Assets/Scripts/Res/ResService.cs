using FH;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResService : FH.ResService
{
    protected override void Awake()
    {
        LogRecorderMgr.Init();
        base.Awake();
    }
}
