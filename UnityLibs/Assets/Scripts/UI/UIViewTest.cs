using FH;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEngine;



public class UIViewTest : MonoBehaviour
{
    public Canvas Canvas;
    private FH.UI.UIButtonView _btn;
    private FH.UI.UIPanelVariantView _view;
    // Start is called before the first frame update
    void Start()
    {
        _btn = FH.UI.UIBaseView.CreateView<FH.UI.UIButtonView>(Canvas.transform);
        _btn.OnClick = _OnOpen;
    }

    public void _OnOpen()
    {
        if (_view != null)
            return;

        this.StartCoroutine(_OpenView());
    }

    private IEnumerator _OpenView()
    {
        IResInstHolder holder = ResMgr.CreateHolder(false, false);
        holder.PreCreate(FH.UI.UIPanelVariantView.C_AssetPath);

        for (; ; )
        {
            var stat = holder.GetStat();
            Log.D(stat.ToString());
            if (stat.IsAllDone)
                break;
            else
                yield return string.Empty;
        }

        _view = FH.UI.UIBaseView.CreateView<FH.UI.UIPanelVariantView>(Canvas.transform, holder);
        _view._btn_0.OnClick = _OnClose;


        //var a1 = FH.ResMgr.Load("Assets/Resources/UI/Sprite/btn_disable1.png");
        //var a = FH.ResMgr.Load("Assets/Resources/UI/Sprite/btn_disable.png");
        //var b = FH.ResMgr.LoadSprite("Assets/Resources/UI/Sprite/btn_disable.png");

        //Debug.LogFormat("{0}", a.Get());
        //Debug.LogFormat("{0}", b.Get());
    }

    public void _OnClose()
    {
        IResInstHolder holder = _view.ResHolder;
        _view.Destroy();
        _view = null;
        holder.Destroy();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
