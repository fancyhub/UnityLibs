using FH;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEngine;



public class UIViewTest : MonoBehaviour
{
    private FH.UI.UIButtonView _btn;
    private FH.UI.UIPanelVariantView _view;
    // Start is called before the first frame update
    void Start()
    {
        _btn = FH.UI.UIBaseView.CreateView<FH.UI.UIButtonView>(this.transform);
        _btn.OnClick = _on_Add;
    }

    public void _on_Add()
    {
        if (_view == null)
        {
            _view = FH.UI.UIBaseView.CreateView<FH.UI.UIPanelVariantView>(this.transform);
            _view._btn_0.OnClick = _onClose;

            var a1 = FH.ResMgr.Load("Assets/Resources/UI/Sprite/btn_disable1.png");
            var a = FH.ResMgr.Load("Assets/Resources/UI/Sprite/btn_disable.png");
            var b = FH.ResMgr.LoadSprite("Assets/Resources/UI/Sprite/btn_disable.png");

            Debug.LogFormat("{0}", a.Get());
            Debug.LogFormat("{0}", b.Get());
        }
    }

    public void _onClose()
    {
        _view.Destroy();
        _view = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
