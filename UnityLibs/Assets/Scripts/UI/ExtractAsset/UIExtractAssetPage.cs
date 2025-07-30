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
            UISceneMgr.AddUpdate(this);
        }

        public void OnUIUpdate()
        {
            if (_Op.IsDone)
            {
                UISceneMgr.ChangeScene<UISceneMain>();
            }

            if(BaseView!=null)
            {
                BaseView._LblDesc.text = _Op.Progress.ToString();
            }            
        }
    }
}