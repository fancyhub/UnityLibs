using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResLoader : MonoBehaviour
{
    public string SpritePath;
    public Sprite ResWithSprite;
    public UnityEngine.Object ResWithDefault;
    private FH.IResInstHolder _resHolder;

    [FH.Omi.Button]
    public void Preload()
    {
        if (_resHolder == null)
            _resHolder = FH.ResMgr.CreateHolder(false, false);
        _resHolder.PreLoad(SpritePath, FH.EResPathType.Sprite);
        _resHolder.PreLoad(SpritePath, FH.EResPathType.Default);
    }

    // Start is called before the first frame update
    [FH.Omi.Button]
    public void LoadSprite()
    {
        ResWithSprite = _resHolder.Load(SpritePath, FH.EResPathType.Sprite)as Sprite;
        ResWithDefault = _resHolder.Load(SpritePath, FH.EResPathType.Default);
    }
}
