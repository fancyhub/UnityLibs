using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResMgr : MonoBehaviour
{
    public FH.ResMgrConfig Config;
    // Start is called before the first frame update
    void Awake()
    {
        FH.ResMgr.InitMgr(new AssetLoader_Resource(), Config);
    }

    // Update is called once per frame
    void Update()
    {
        FH.ResMgr.Update();
    }

    public void OnDestroy()
    {
        FH.ResMgr.Destroy();
    }
}
