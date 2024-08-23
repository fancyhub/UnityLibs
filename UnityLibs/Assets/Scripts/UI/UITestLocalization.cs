using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH.UI;

public sealed class UIPageTestLocalization : UIPage
{
    private UITestLocalizationView _view;

    public static UIPageTestLocalization Create()
    {
        UIPageTestLocalization ret = new UIPageTestLocalization();
        ret.UIOpen();
        return ret;
    }

    protected override void OnUIInit()
    {
        _view = CreateView<UITestLocalizationView>();
        _view._btn_0.OnClick = () =>
        {
            LocMgr.ChangeLang("en");
        };
        _view._btn_1.OnClick = () =>
        {
            LocMgr.ChangeLang("zh-Hans");
        };
    }
    protected override void OnUIPrepareRes(IResInstHolder holder)
    {
        holder.PreCreate(UITestLocalizationView.CPath);
    }
    protected override void OnUIShow()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUIHide()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUIClose()
    {
        throw new System.NotImplementedException();
    }
}

