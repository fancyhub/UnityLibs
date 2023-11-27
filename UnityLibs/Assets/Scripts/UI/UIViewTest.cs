using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
