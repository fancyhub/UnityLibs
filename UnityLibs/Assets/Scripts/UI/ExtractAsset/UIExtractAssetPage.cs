using System;
using System.Collections.Generic;
using FH.UI;
using FH;
namespace Game
{
    public class UIExtractAssetPage : FH.UI.UIPageBase<Game.UIExtractAssetView>, IUIUpdater
    {
        private List<FileDownloadJobInfo> _AllDownloadJobs = new List<FileDownloadJobInfo>();
        private FH.ExtractStreamingAssetsOperation _Op;
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            _Op = FileMgr.GetExtractOperation();
            UIMgr.UpdateList += this;
        }

        public void OnUIUpdate(float dt)
        {
            if (_Op.IsDone)
            {
                UIMgr.ChangeScene<UISceneMain>();
            }

            if (BaseView != null)
            {
                BaseView._LblDesc.text = _Op.Progress.ToString();
            }
        }
    }
}