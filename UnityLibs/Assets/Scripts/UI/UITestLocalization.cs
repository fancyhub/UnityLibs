using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH.UI;

public sealed class UIPageTestLocalization : CPoolItemBase
{
    private Transform _UIRoot;
    private UITestLocalizationView _view;

    public static UIPageTestLocalization Create(Transform root)
    {
        var ret = GPool.New<UIPageTestLocalization>();
        ret._UIRoot = root;
        ret._Open();
        return ret;
    }

    private void _Open()
    {
        _view = FH.UI.UIBaseView.CreateView<UITestLocalizationView>(_UIRoot);
        _view._btn_0.OnClick = () =>
        {
            LocMgr.ChangeLang("en");
        };
        _view._btn_1.OnClick = () =>
        {
            LocMgr.ChangeLang("zh-Hans");
        };
    }

    protected override void OnPoolRelease()
    {
        _view?.Destroy();
        _view = null;
    }
}

