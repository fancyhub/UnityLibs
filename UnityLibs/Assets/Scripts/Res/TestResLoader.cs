using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResLoader : MonoBehaviour
{
    public string SpritePath;
    public Sprite ResWithSprite;
    public UnityEngine.Object ResWithDefault;
    private FH.IResInstHolder _resHolder;


    public string SameAssetPath1;
    public string SameAssetPath2;
    public UnityEngine.Object SameAsset1;
    public UnityEngine.Object SameAsset2;

    [FH.Omi.Button]
    public void Preload()
    {
        if (_resHolder == null)
            _resHolder = FH.ResMgr.CreateHolder(false, false);
        _resHolder.PreLoad(SpritePath, FH.EResPathType.Sprite);
        _resHolder.PreLoad(SpritePath, FH.EResPathType.Default);
    }

    [FH.Omi.Button]
    public void LoadSprite()
    {
        ResWithSprite = _resHolder.Load(SpritePath, FH.EResPathType.Sprite) as Sprite;
        ResWithDefault = _resHolder.Load(SpritePath, FH.EResPathType.Default);
    }

    [FH.Omi.Button]
    public void DestroyHolder()
    {
        _resHolder?.Destroy();
        _resHolder = FH.ResMgr.CreateHolder(false, false);
    }


    [FH.Omi.Button]
    public void LoadSameAsset1()
    {
        SameAsset1 = FH.ResMgr.Load(SameAssetPath1).Get();
    }

    [FH.Omi.Button]
    public void LoadSameAsset2()
    {
        SameAsset2 = FH.ResMgr.Load(SameAssetPath2).Get();
    }
}
