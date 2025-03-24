using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH.UI;

public sealed class UIPageTestLocalization : UIPageBase
{
    //private UITestLocalizationView _view;

    public static UIPageTestLocalization Create()
    {
        UIPageTestLocalization ret = new UIPageTestLocalization();
        ret.UIOpen();
        return ret;
    }

    protected override void OnUI2Init()
    {
        //_view = CreateView<UITestLocalizationView>();
        //_view._btn_0.OnClick = () =>
        //{
        //    LocMgr.ChangeLang("en");
        //};
        //_view._btn_1.OnClick = () =>
        //{
        //    LocMgr.ChangeLang("zh-Hans");
        //};
    }
    protected override void OnUI1PrepareRes(IResInstHolder holder)
    {
        //holder.PreCreate(UITestLocalizationView.CPath);
    }
    protected override void OnUI3Show()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUI4Hide()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUI5Close()
    {
        throw new System.NotImplementedException();
    }
}

